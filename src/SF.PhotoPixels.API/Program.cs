using System.Diagnostics;
using System.Runtime.InteropServices;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.HttpOverrides;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc.Authorization;
using Microsoft.Extensions.Options;
using Microsoft.OpenApi.Models;
using Oakton;
using Serilog;
using Serilog.Debugging;
using Serilog.Sinks.Grafana.Loki;
using SF.PhotoPixels.API.Extensions;
using SF.PhotoPixels.API.Middlewares;
using SF.PhotoPixels.API.Security.RequireAdminRole;
using SF.PhotoPixels.Application;
using SF.PhotoPixels.Application.Core;
using SF.PhotoPixels.Application.Security;
using SF.PhotoPixels.Application.Security.BearerToken;
using SF.PhotoPixels.Application.VersionMigrations;
using SF.PhotoPixels.Domain.Entities;
using SF.PhotoPixels.Infrastructure;
using SF.PhotoPixels.Infrastructure.Options;
using SF.PhotoPixels.Infrastructure.Services.TusService;
using SF.PhotoPixels.Infrastructure.Stores;
using tusdotnet;
using tusdotnet.Helpers;
using tusdotnet.Models;
using tusdotnet.Models.Configuration;
using tusdotnet.Models.Expiration;
using tusdotnet.Stores;

const string appName = "PhotoPixels.io";

var builder = WebApplication.CreateBuilder(args);

var telemetryConfiguration = new TelemetryConfiguration
{
    AppName = appName,
};

builder.Configuration.Bind(TelemetryConfiguration.SectionName, telemetryConfiguration);

var loggerConfiguration = new LoggerConfiguration()
    .ReadFrom.Configuration(builder.Configuration)
    .WriteTo.Console()
    .Enrich.FromLogContext()
    .Enrich.WithProperty("ApplicationName", appName)
    .Enrich.WithProperty("Device", Environment.MachineName)
    .WriteTo.Async(x =>
        x.File(
            Path.Combine(GetRootDirectory(), "photopixels", "log-.log"),
            rollingInterval: RollingInterval.Day,
            rollOnFileSizeLimit: true
        )
    );
SelfLog.Enable(msg => Debug.WriteLine(msg));

if (!string.IsNullOrWhiteSpace(telemetryConfiguration.LokiUrl))
{
    loggerConfiguration.WriteTo.GrafanaLoki(telemetryConfiguration.LokiUrl, new[] { new LokiLabel { Key = "ApplicationName", Value = appName } });
}

Log.Logger = loggerConfiguration.CreateBootstrapLogger();
builder.Logging.ClearProviders();
builder.Logging.AddSerilog();
builder.Host.UseSerilog();

builder.Services.AddTelemetry(telemetryConfiguration);

builder.Services.AddScoped<IAuthorizationHandler, RequireAdminRoleAuthorizationHandler>();

builder.Services.AddAuthorization(options =>
    {
        var lockedDown = new AuthorizationPolicyBuilder().RequireAuthenticatedUser().Build();
        options.FallbackPolicy = lockedDown;
        options.DefaultPolicy = lockedDown;

        options.AddPolicy(RequireAdminRoleAttribute.PolicyName, policyBuilder => policyBuilder.Requirements.Add(new RequireAdminRoleRequirement()));
    })
    .AddControllers(options => { options.Filters.Add(new AuthorizeFilter()); });

builder.Services.AddInfrastructureServices(builder.Configuration, builder.Environment.IsDevelopment())
    .AddApplicationServices(builder.Configuration);

builder.Services.AddIdentityCore<User>(opt =>
    {
        opt.User.RequireUniqueEmail = true;
        opt.Password.RequiredLength = 8;
        opt.ClaimsIdentity.UserIdClaimType = CustomClaims.Id;
        opt.ClaimsIdentity.EmailClaimType = CustomClaims.Email;
    })
    .AddUserStore<UserStore>()
    .AddSignInManager()
    .AddClaimsPrincipalFactory<PrincipalFactory>()
    .AddDefaultTokenProviders();

builder.Services.Configure<SmtpOptions>(builder.Configuration.GetSection("EmailConfiguration"));
builder.Services.AddAuthentication(options =>
    {
        options.DefaultAuthenticateScheme = BearerTokenDefaults.AuthenticationScheme;
        options.DefaultChallengeScheme = BearerTokenDefaults.AuthenticationScheme;
        options.DefaultScheme = BearerTokenDefaults.AuthenticationScheme;
        options.DefaultSignInScheme = BearerTokenDefaults.AuthenticationScheme;
        options.DefaultSignOutScheme = BearerTokenDefaults.AuthenticationScheme;
    })
    .AddCustomBearerToken(options => { options.BearerTokenExpiration = TimeSpan.FromMinutes(10); });

builder.Services.AddSwaggerGen(options =>
{
    options.EnableAnnotations();
    options.CustomSchemaIds(type => type.ToString());

    options.SwaggerDoc("v1", new OpenApiInfo
    {
        Version = "v1",
        Title = appName,
        Description = "PhotoPixels.io - an open-source media backup platform",
    });
    options.CustomSchemaIds(type => type.FullName);
});

builder.Services.AddSwaggerGen(options =>
{
    const string bearer = "Bearer";

    options.AddSecurityRequirement(new OpenApiSecurityRequirement
    {
        {
            new OpenApiSecurityScheme
            {
                Reference = new OpenApiReference
                {
                    Type = ReferenceType.SecurityScheme,
                    Id = bearer,
                },
                Scheme = bearer,
                Name = bearer,
                In = ParameterLocation.Header,
            },
            new List<string>()
        },
    });

    options.CustomSchemaIds(type => type.FullName!.Replace("+", "_", StringComparison.OrdinalIgnoreCase));
    options.AddSecurityDefinition(bearer, new OpenApiSecurityScheme
    {
        Description = $"Bearer Authorization header using the {bearer} scheme. Example: \"Authorization: {bearer} {{token}}\"",
        Name = "Authorization",
        In = ParameterLocation.Header,
        Type = SecuritySchemeType.ApiKey,
        Scheme = bearer,
    });
});
builder.Host.ApplyOaktonExtensions();


var app = builder.Build();
app.UseSerilogRequestLogging();
app.UseForwardedHeaders(new ForwardedHeadersOptions
{
    ForwardedHeaders = ForwardedHeaders.All,
});

app.UseTelemetry(telemetryConfiguration);

// Configure the HTTP request pipeline.
if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Error");
    // The default HSTS value is 30 days. You may want to change this for production scenarios, see https://aka.ms/aspnetcore-hsts.
    app.UseHsts();
}

app.UseCors(cfg =>
{
    cfg.SetIsOriginAllowed(_ => true)
        .AllowAnyMethod()
        .AllowAnyHeader()
        .AllowCredentials()
        .WithExposedHeaders();
});

app.Use((context, next) =>
{
    var prefix = GetPrefix(context);

    context.Request.PathBase = new PathString(prefix);

    return next(context);
});

app.UseSwagger();
app.UseSwaggerUI(c => { c.SwaggerEndpoint("v1/swagger.json", $"{appName} v1"); });

app.MapGet("/", () => Results.Redirect("/swagger")).AllowAnonymous();

app.UseHttpsRedirection();

app.UseRouting();

app.UseAuthentication();
app.UseAuthorization();

app.UseMiddleware<ExceptionHandlingMiddleware>();



app.MapControllers();

await app.ExecuteMigrations();

await using (var scope = app.Services.CreateAsyncScope())
{
    var intiService = scope.ServiceProvider.GetRequiredService<Initialization>();
    await intiService.FirstTimeSetup();
}

await app.RunOaktonCommands(args);

await app.RunAsync();

public partial class Program
{
    private static string GetRootDirectory()
    {
        return RuntimeInformation.IsOSPlatform(OSPlatform.Linux) ? Path.Combine(Path.DirectorySeparatorChar + "var", "log") : Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments);
    }

    private static string GetPrefix(HttpContext context)
    {
        var prefix = string.Empty;

        if (!bool.TryParse(Environment.GetEnvironmentVariable("DOTNET_RUNNING_IN_CONTAINER"), out var isRunningInContainerBool))
        {
            return prefix;
        }

        var forwardedHeaders = context.RequestServices.GetService<IOptions<ForwardedHeadersOptions>>()?.Value;

        if (forwardedHeaders is null)
        {
            return prefix;
        }

        var hasProxyHeaders = context.Request.Headers.ContainsKey(forwardedHeaders.ForwardedForHeaderName);

        if (isRunningInContainerBool && hasProxyHeaders)
        {
            prefix = "/api";
        }

        return prefix;
    }
}