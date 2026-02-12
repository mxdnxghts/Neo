var builder = DistributedApplication.CreateBuilder(args);

var apiService = builder.AddProject<Projects.NeoTelemetry_ApiService>("apiservice")
    .WithHttpHealthCheck("/health");

builder.AddProject<Projects.NeoTelemetry_Web>("webfrontend")
    .WithExternalHttpEndpoints()
    .WithHttpHealthCheck("/health")
    .WithReference(apiService)
    .WaitFor(apiService);

builder.AddProject<Projects.NeoAndroidApp>("neoandroidapp");

builder.Build().Run();
