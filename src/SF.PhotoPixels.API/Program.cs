using HealthChecks.UI.Client;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Diagnostics.HealthChecks;
using Microsoft.AspNetCore.HttpOverrides;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc.Authorization;
using Oakton;
using Serilog;
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
using SF.PhotoPixels.Infrastructure.Stores;

var builder = WebApplication.CreateBuilder(args);

var telemetryConfiguration = builder.ConfigureTelemetry();

builder.ConfigureLogging(telemetryConfiguration);

builder.Services.AddTelemetry(telemetryConfiguration);

builder.ConfigureHealthChecks();

builder.Services.AddScoped<IAuthorizationHandler, RequireAdminRoleAuthorizationHandler>();

builder.Services.AddAuthorization(options =>
    {
        var lockedDown = new AuthorizationPolicyBuilder().RequireAuthenticatedUser().Build();
        options.FallbackPolicy = lockedDown;
        options.DefaultPolicy = lockedDown;

        options.AddPolicy(RequireAdminRoleAttribute.PolicyName, policyBuilder => policyBuilder.Requirements.Add(new RequireAdminRoleRequirement()));
    })
    .AddControllers(options =>
    {
        options.Filters.Add(new AuthorizeFilter());
    });

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
    .AddCustomBearerToken(options =>
    {
        options.BearerTokenExpiration = TimeSpan.FromMinutes(10);
    });

builder.Services.AddSwaggerDocumentation();

builder.Host.ApplyOaktonExtensions();

builder.ConfigureWebServersUpperLimitOptions();

var app = builder.Build();

app.ConfigureFFmpegVideoSupport();

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
    var prefix = context.GetPrefix();

    context.Request.PathBase = new PathString(prefix);

    return next(context);
});

app.UseSwaggerDocumentation();

app.UseHttpsRedirection();

app.UseRouting();

app.MapHealthChecks("/health", new HealthCheckOptions
    {
        ResponseWriter = UIResponseWriter.WriteHealthCheckUIResponse
    })
    .AllowAnonymous();

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

public partial class Program;