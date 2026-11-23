using Azure.Storage.Blobs;
using Azure.Storage.Blobs.Models;
using Datahub.TrainingPortal.Application.Interfaces;

namespace Datahub.TrainingPortal.Infrastructure.Storage;

public class AzureBlobStorageService(BlobContainerClient containerClient) : IFileStorageService
{
    public async Task UploadAsync(string blobPath, Stream content, string contentType)
    {
        var blobClient = containerClient.GetBlobClient(blobPath);
        await blobClient.UploadAsync(content, new BlobHttpHeaders { ContentType = contentType });
    }

    public async Task<Stream> DownloadAsync(string blobPath)
    {
        var blobClient = containerClient.GetBlobClient(blobPath);

        if (!await blobClient.ExistsAsync())
        {
            throw new FileNotFoundException($"Blob '{blobPath}' was not found.", blobPath);
        }

        var download = await blobClient.DownloadStreamingAsync();
        return download.Value.Content;
    }

    public async Task DeleteAsync(string blobPath)
    {
        var blobClient = containerClient.GetBlobClient(blobPath);
        await blobClient.DeleteIfExistsAsync();
    }
}
