using Projects;
using SF.PhotoPixels.Aspire;

var builder = DistributedApplication.CreateBuilder(args);

var pgUsername = builder.AddParameter("DbUsername");
var pgPassword = builder.AddParameter("DbPassword", secret: true);
var adminEmail = builder.AddParameter("AdminEmail");
var adminPassword = builder.AddParameter("AdminPassword", secret: true);
var webAppLocationParameter = builder.AddParameter("WebAppLocation", value: EmptyStringParameterDefault.Instance);

var db = builder.AddPostgres("pgsql", pgUsername, pgPassword)
    .WithImage("postgres", "14.3")
    .WithLifetime(ContainerLifetime.Persistent)
    .WithDataVolume("sf-photos-db")
    .AddDatabase("photosdb");

var app = builder.AddProject<SF_PhotoPixels_API>("API")
    .WaitFor(db)
    .WithReference(db, "PhotosMetadata")
    .WithExternalHttpEndpoints()
    .WithHttpHealthCheck("/health")
    .WithEnvironment("Admin__Email", adminEmail)
    .WithEnvironment("Admin__Password", adminPassword)
    .WithEnvironment("Telemetry__Enabled", "true")
    .WithOtlpExporter(OtlpProtocol.Grpc);

var webAppLocation = await webAppLocationParameter.Resource.GetValueAsync(CancellationToken.None);

if (!string.IsNullOrWhiteSpace(webAppLocation))
{
    builder.AddJavaScriptApp("webapp", webAppLocation, "start")
        .WithReference(app, "REACT_APP_BACKEND");
}

await builder.Build().RunAsync();