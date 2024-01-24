using Azure.Storage.Blobs;
using Azure.Storage.Blobs.Models;
using MimeMapping;

namespace ChatApplication.StorageProviders.Azure;

internal class AzureStorageProvider : IStorageProvider
{
    private readonly BlobServiceClient blobServiceClient;
    private readonly AzureStorageSettings settings;

    public AzureStorageProvider(AzureStorageSettings settings)
    {
        blobServiceClient = new BlobServiceClient(settings.ConnectionString);
        this.settings = settings;
    }

    public async Task DeleteAsync(string path)
    {
        var blobContainerClient = blobServiceClient.GetBlobContainerClient(settings.ContainerName);
        await blobContainerClient.DeleteBlobIfExistsAsync(path);
    }

    public async Task<Stream?> ReadAsync(string path)
    {
        var blobClient = await GetBlobClientAsync(path);
        var exists = await blobClient.ExistsAsync();

        if (!exists)
        {
            return null;
        }

        var stream = await blobClient.OpenReadAsync();
        return stream;
    }

    public async Task UploadAsync(Stream stream, string path, bool overwrite = false)
    {
        var blobClient = await GetBlobClientAsync(path, true);
        if (!overwrite)
        {
            var exists = await blobClient.ExistsAsync();
            if (exists)
            {
                throw new IOException($"The file {path} already exists");
            }
        }

        var headers = new BlobHttpHeaders
        {
            ContentType = MimeUtility.GetMimeMapping(path)
        };

        stream.Position = 0;
        await blobClient.UploadAsync(stream, headers);
    }

    private async Task<BlobClient> GetBlobClientAsync(string path, bool createIfNotExists = false)
    {
        var blobContainerClient = blobServiceClient.GetBlobContainerClient(settings.ContainerName);
        if (createIfNotExists)
        {
            await blobContainerClient.CreateIfNotExistsAsync(PublicAccessType.None);
        }

        return blobContainerClient.GetBlobClient(path);
    }
}