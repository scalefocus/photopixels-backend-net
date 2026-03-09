namespace SF.PhotoPixels.API.Extensions;

public class TelemetryConfiguration
{
    public static readonly string SectionName = "Telemetry";

    public required string AppName { get; init; }

    public bool Enabled { get; set; }

    public string PrometheusScrapePath { get; set; } = "/internal/metrics";

    public Uri? OtelExporterUri { get; set; }

    public string? LokiUrl { get; set; }
}