// Copyright (c) Microsoft. All rights reserved.

using Aspire.Hosting.Azure;
using Azure.Provisioning;
using Azure.Provisioning.Primitives;
using Azure.Provisioning.Storage;
using Microsoft.Extensions.DependencyInjection;

public static class SecurityExtensions
{
    public static IDistributedApplicationBuilder WithSecureInfrastructure(
        this IDistributedApplicationBuilder builder)
    {
        builder.Services.Configure<AzureProvisioningOptions>(options =>
            options.ProvisioningBuildOptions.InfrastructureResolvers.Add(new RequireSecureStorageAccess()));

        return builder;
    }
}

public class RequireSecureStorageAccess : InfrastructureResolver
{
    public override void ResolveProperties(ProvisionableConstruct construct, ProvisioningBuildOptions options)
    {
        base.ResolveProperties(construct, options);

        foreach (var provisionable in construct.GetProvisionableResources())
        {
            Log($"provisionable Type: {provisionable.GetType().FullName}");
            switch (provisionable)
            {
                case StorageAccount storage:
                    SecureStorageAccount(storage);
                    break;
                case BlobService blobService:
                    SecureStorageAccount(blobService);
                    break;
            }
        }

        Log($"construct Type: {construct.GetType().FullName}");
        switch (construct)
        {
            case StorageAccount storage:
                SecureStorageAccount(storage);
                break;
            case BlobService blobService:
                SecureStorageAccount(blobService);
                break;
        }
    }

    private static void SecureStorageAccount(BlobService? blobService)
    {
        SecureStorageAccount(blobService?.Parent);
    }

    private static void SecureStorageAccount(StorageAccount? storage)
    {
        if (storage == null)
        {
            return;
        }

        Log($"  - AllowBlobPublicAccess is : {storage.AllowBlobPublicAccess.Value}");
        Log($"  - EnableHttpsTrafficOnly is: {storage.EnableHttpsTrafficOnly.Value}");
        Log($"  - AllowSharedKeyAccess is  : {storage.AllowSharedKeyAccess.Value}");

        if (!((IBicepValue)storage.AllowBlobPublicAccess).IsOutput)
        {
            Log("  - Setting AllowBlobPublicAccess to false");
            storage.AllowBlobPublicAccess = false;
        }

        if (!((IBicepValue)storage.EnableHttpsTrafficOnly).IsOutput)
        {
            Log("  - Setting EnableHttpsTrafficOnly to true");
            storage.EnableHttpsTrafficOnly = true;
        }

        if (!((IBicepValue)storage.AllowSharedKeyAccess).IsOutput)
        {
            Log("  - Setting AllowSharedKeyAccess to false");
            storage.AllowSharedKeyAccess = false;
        }
    }

    private static void Log(string text)
    {
        File.AppendAllText("provision.log", text + "\n");
    }
}