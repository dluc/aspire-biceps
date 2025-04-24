using Aspire.Hosting.Azure;
using Microsoft.Extensions.DependencyInjection;
using Projects;

var builder = DistributedApplication.CreateBuilder(args);

builder.Services.Configure<AzureProvisioningOptions>(options =>
    options.ProvisioningBuildOptions.InfrastructureResolvers.Add(new RequireSecureStorageAccess()));

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

var apiService = builder.AddProject<ApiService>("apiservice")
    .WithHttpsHealthCheck("/health");

builder.AddProject<Web>("webfrontend")
    .WithExternalHttpEndpoints()
    .WithHttpsHealthCheck("/health")
    .WithReference(apiService)
    .WaitFor(apiService);

builder.Build().Run();