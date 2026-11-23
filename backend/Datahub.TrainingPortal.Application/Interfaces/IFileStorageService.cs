namespace Datahub.TrainingPortal.Application.Interfaces;

public interface IFileStorageService
{
    Task UploadAsync(string blobPath, Stream content, string contentType);
    Task<Stream> DownloadAsync(string blobPath);
    Task DeleteAsync(string blobPath);
}
