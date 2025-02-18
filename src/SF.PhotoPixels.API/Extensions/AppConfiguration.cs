using Serilog.Debugging;
using Serilog.Sinks.Grafana.Loki;
using Serilog;
using HealthChecks.Network.Core;
using System.Runtime.InteropServices;
using System.Diagnostics;
using Microsoft.Extensions.Options;

namespace SF.PhotoPixels.API.Extensions
{
    public static class AppConfiguration
    {
        const string appName = "PhotoPixels.io";

        public static string GetAppName() => appName;

        public static TelemetryConfiguration ConfigureTelemetry(this WebApplicationBuilder builder)
        {
            var telemetryConfiguration = new TelemetryConfiguration
            {
                AppName = appName,
            };
            builder.Configuration.Bind(TelemetryConfiguration.SectionName, telemetryConfiguration);
            return telemetryConfiguration;
        }

        public static void ConfigureLogging(this WebApplicationBuilder builder, TelemetryConfiguration telemetryConfiguration)
        {
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
        }

        public static void ConfigureHealthChecks(this WebApplicationBuilder builder)
        {
            var smtpOptions = builder.Configuration.GetSection("EmailConfiguration");

            builder.Services.AddHealthChecks()
                .AddNpgSql(builder.Configuration.GetConnectionString("PhotosMetadata"))
                .AddPingHealthCheck(options => options.AddHost("localhost", 1000))
                .AddSmtpHealthCheck(setup =>
                {
                    setup.Host = smtpOptions.GetValue<string>("Host");
                    setup.Port = smtpOptions.GetValue<int>("Port");
                    setup.ConnectionType = smtpOptions.GetValue<bool>("UseSsl") == true ? SmtpConnectionType.SSL : SmtpConnectionType.TLS;
                    setup.LoginWith(smtpOptions.GetValue<string>("Username"), smtpOptions.GetValue<string>("Password"));
                });
        }

        public static string GetPrefix(this HttpContext context)
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

        public static string GetRootDirectory()
        {
            return RuntimeInformation.IsOSPlatform(OSPlatform.Linux) ? Path.Combine(Path.DirectorySeparatorChar + "var", "log") : Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments);
        }
    }
}
