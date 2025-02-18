using System.Diagnostics;
using Microsoft.OpenApi.Models;
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

    public static IServiceCollection AddSwaggerDocumentation(this IServiceCollection services)
    {
        services.AddSwaggerGen(options =>
        {
            options.EnableAnnotations();
            options.CustomSchemaIds(type => type.ToString());

            options.SwaggerDoc("v1", new OpenApiInfo
            {
                Version = "v1",
                Title = AppConfiguration.GetAppName(),
                Description = "PhotoPixels.io - an open-source media backup platform",
            });
            options.CustomSchemaIds(type => type.FullName);
        });

        services.AddSwaggerGen(options =>
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

        return services;
    }

    public static WebApplication UseSwaggerDocumentation(this WebApplication app)
    {
        app.UseSwagger();
        app.UseSwaggerUI(c => { c.SwaggerEndpoint("v1/swagger.json", $"{AppConfiguration.GetAppName()} v1"); });

        app.MapGet("/", () => Results.Redirect("/swagger")).AllowAnonymous();

        return app;
    }
}