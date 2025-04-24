using Projects;

var builder = DistributedApplication.CreateBuilder(args);

builder.WithSecureInfrastructure();

if (builder.ExecutionContext.IsPublishMode)
{
    builder.AddQdrant("qdrant")
        .WithDataVolume();
}
else
{
    builder
        .AddQdrant("qdrant", apiKey: builder.AddParameter("qdrant-Key", "changeme"))
        .WithDataBindMount(Path.Join("/tmp", "qdrant-data"));
}

var apiService = builder.AddProject<solution_ApiService>("apiservice")
    .WithHttpsHealthCheck("/health");

builder.AddProject<solution_Web>("webfrontend")
    .WithExternalHttpEndpoints()
    .WithHttpsHealthCheck("/health")
    .WithReference(apiService)
    .WaitFor(apiService);

builder.Build().Run();