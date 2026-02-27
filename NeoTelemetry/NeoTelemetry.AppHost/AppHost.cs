var builder = DistributedApplication.CreateBuilder(args);

// Add Redis for distributed caching (optional, for production scenarios)
// var cache = builder.AddRedis("cache");

// Add Neo API service with telemetry
var apiService = builder.AddProject<Projects.NeoTelemetry_ApiService>("apiservice")
    .WithHttpHealthCheck("/health")
    // .WithReference(cache)
    // .WaitFor(cache)
    .WithExternalHttpEndpoints();

// Add Web frontend
builder.AddProject<Projects.NeoTelemetry_Web>("webfrontend")
    .WithExternalHttpEndpoints()
    .WithHttpHealthCheck("/health")
    .WithReference(apiService)
    .WaitFor(apiService);

// Add MAUI app (mobile/desktop) - Note: MAUI apps run separately, not orchestrated by Aspire
// builder.AddProject<Projects.NeoAndroidApp>("neoandroidapp");

// The Aspire Dashboard is automatically available at:
// - HTTP: http://localhost:15888
// - HTTPS: https://localhost:15889
// Configure OTEL_EXPORTER_OTLP_ENDPOINT to send telemetry to the dashboard

builder.Build().Run();