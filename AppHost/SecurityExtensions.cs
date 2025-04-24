// Copyright (c) Microsoft. All rights reserved.

using Azure.Provisioning;
using Azure.Provisioning.Primitives;
using Azure.Provisioning.Storage;

public class RequireSecureStorageAccess : InfrastructureResolver
{
    public override void ResolveProperties(ProvisionableConstruct construct, ProvisioningBuildOptions options)
    {
        Log($"{nameof(RequireSecureStorageAccess)}.ResolveProperties start");

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

        Log(storage);

        if (!((IBicepValue)storage.AllowBlobPublicAccess).IsOutput)
        {
            storage.AllowBlobPublicAccess = false;
        }

        if (!((IBicepValue)storage.EnableHttpsTrafficOnly).IsOutput)
        {
            storage.EnableHttpsTrafficOnly = true;
        }

        if (!((IBicepValue)storage.AllowSharedKeyAccess).IsOutput)
        {
            Log("  - Setting AllowSharedKeyAccess to false");
            storage.AllowSharedKeyAccess = false;
        }

        Log(storage);
    }

    private const string LogFile = "provision.log";

    private static void Log(StorageAccount storage)
    {
        try
        {
            File.AppendAllText(LogFile, $"  - AllowBlobPublicAccess is : {storage.AllowBlobPublicAccess.Value}\n");
            File.AppendAllText(LogFile, $"  - EnableHttpsTrafficOnly is : {storage.EnableHttpsTrafficOnly.Value}\n");
            File.AppendAllText(LogFile, $"  - AllowSharedKeyAccess is : {storage.AllowSharedKeyAccess.Value}\n");
        }
        catch (Exception e)
        {
            File.AppendAllText(LogFile, e.ToString());
        }
    }

    private static void Log(string text)
    {
        try
        {
            File.AppendAllText(LogFile, $"{text}\n");
        }
        catch (Exception e)
        {
            File.AppendAllText(LogFile, e.ToString());
        }
    }
}