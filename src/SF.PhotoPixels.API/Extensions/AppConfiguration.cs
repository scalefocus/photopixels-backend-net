using Serilog.Debugging;
using Serilog.Sinks.Grafana.Loki;
using Serilog;
using HealthChecks.Network.Core;
using System.Runtime.InteropServices;
using System.Diagnostics;
using Microsoft.Extensions.Options;
using Microsoft.AspNetCore.Server.Kestrel.Core;
using FFMpegCore;
using Microsoft.Extensions.Hosting;

namespace SF.PhotoPixels.API.Extensions
{
    public static class AppConfiguration
    {
        private const string appName = "PhotoPixels.io";
        private const int defaultUpperLimit = 50 * 1024 * 1024;
        private const string ffmpegWindowsBinaryFolder = @"C:\ffmpeg\bin";

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

        public static void ConfigureWebServersUpperLimitOptions(this WebApplicationBuilder builder)
        {
            var upperLimit = builder.Configuration.GetValue<int>("UploadFileUpperLimit");
            if (upperLimit == 0)
            {
                upperLimit = defaultUpperLimit; // default value 50 MB
            }

            builder.Services.Configure<Microsoft.AspNetCore.Http.Features.FormOptions>(options =>
            {
                options.MultipartBodyLengthLimit = upperLimit;
            });

            builder.Services.Configure<IISServerOptions>(options =>
            {
                options.MaxRequestBodySize = upperLimit;
            });

            builder.Services.Configure<KestrelServerOptions>(options =>
            {
                options.Limits.MaxRequestBodySize = upperLimit;
            });
        }

        public static void ConfigureFFmpegVideoSupport(this WebApplication app)
        {
            var logger = app.Services.GetRequiredService<ILogger<Program>>();

            // install ffmpeg on windows os
            if (RuntimeInformation.IsOSPlatform(OSPlatform.Windows))
            {
                var scriptPath = Path.Combine(Environment.CurrentDirectory, "Install-FFmpeg.ps1");
                var processInfo = new ProcessStartInfo("powershell.exe", $"-NoProfile -ExecutionPolicy Bypass -File \"{scriptPath}\"")
                {
                    RedirectStandardOutput = true,
                    UseShellExecute = false,
                    CreateNoWindow = true
                };

                using (var process = Process.Start(processInfo))
                {
                    logger.LogInformation("Starting FFmpeg configuration...");
                    process.WaitForExit();
                    var output = process.StandardOutput.ReadToEnd();
                    logger.LogInformation(output);
                }
            }

            // configure ffmpeg global options
            var binaryFolder = RuntimeInformation.IsOSPlatform(OSPlatform.Linux) ?
                "/bin"
                : ffmpegWindowsBinaryFolder;

            var temporaryFilesFolder = RuntimeInformation.IsOSPlatform(OSPlatform.Linux) ?
                "/tmp" :
                Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments), "sf-photos", "temp");
            if (!Directory.Exists(temporaryFilesFolder))
            {
                Directory.CreateDirectory(temporaryFilesFolder);
            }

            FFOptions ffOptions = new()
            {
                BinaryFolder = binaryFolder,
                TemporaryFilesFolder = temporaryFilesFolder
            };
            GlobalFFOptions.Configure(ffOptions);
        }
    }
}
