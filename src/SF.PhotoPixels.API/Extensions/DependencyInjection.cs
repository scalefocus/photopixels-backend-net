using System.Diagnostics;
using Npgsql;
using OpenTelemetry;
using OpenTelemetry.Exporter;
using OpenTelemetry.Metrics;
using OpenTelemetry.Resources;
using OpenTelemetry.Trace;

namespace SF.PhotoPixels.API.Extensions;

public static class DependencyInjection
{
    public static IServiceCollection AddTelemetry(this IServiceCollection services, TelemetryConfiguration telemetryConfiguration)
    {
        if (!telemetryConfiguration.Enabled)
        {
            return services;
        }

        services.AddOpenTelemetry()
            .ConfigureResource(rb =>
            {
                rb.AddService(telemetryConfiguration.AppName)
                    .AddTelemetrySdk()
                    .AddEnvironmentVariableDetector();
            })
            .WithMetrics(x =>
            {
                x.AddAspNetCoreInstrumentation()
                    .AddMeter("Microsoft.AspNetCore.Hosting")
                    .AddMeter("Microsoft.AspNetCore.Server.Kestrel")
                    .AddMeter("Npgsql")
                    .AddPrometheusExporter(cfg => { cfg.ScrapeEndpointPath = telemetryConfiguration.PrometheusScrapePath; })
                    .AddHttpClientInstrumentation()
                    .AddRuntimeInstrumentation()
                    .AddProcessInstrumentation();
            })
            .WithTracing(x =>
            {
                x.AddSource("SF.PhotoPixels.*")
                    .AddNpgsql()
                    .AddAspNetCoreInstrumentation(options =>
                    {
                        options.RecordException = true;
                        options.Filter = context => context.Request.Path != telemetryConfiguration.PrometheusScrapePath;
                    })
                    .AddHttpClientInstrumentation(httpClientInstrumentationOptions =>
                    {
                        httpClientInstrumentationOptions.RecordException = true;
                        httpClientInstrumentationOptions.FilterHttpRequestMessage =
                            request => request.RequestUri?.AbsoluteUri.Contains("localhost", StringComparison.OrdinalIgnoreCase) != true;
                    })
                    .AddOtlpExporter(o =>
                    {
                        o.Endpoint = telemetryConfiguration.OtelExporterUri;
                        o.Protocol = OtlpExportProtocol.Grpc;
                        o.ExportProcessorType = ExportProcessorType.Batch;
                        o.BatchExportProcessorOptions = new BatchExportProcessorOptions<Activity>
                        {
                            MaxQueueSize = 2048,
                            ScheduledDelayMilliseconds = 5000,
                            ExporterTimeoutMilliseconds = 30000,
                            MaxExportBatchSize = 512,
                        };
                    });
#if DEBUG
                x.AddConsoleExporter();
#endif
            });


        return services;
    }

    public static WebApplication UseTelemetry(this WebApplication host, TelemetryConfiguration telemetryConfiguration)
    {
        if (!telemetryConfiguration.Enabled)
        {
            return host;
        }

        host.UseOpenTelemetryPrometheusScrapingEndpoint(context => context.Request.Path == telemetryConfiguration.PrometheusScrapePath);


        return host;
    }
}