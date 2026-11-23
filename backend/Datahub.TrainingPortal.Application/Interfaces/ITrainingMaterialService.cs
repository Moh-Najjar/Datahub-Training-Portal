using Datahub.TrainingPortal.Application.DTOs.Materials;

namespace Datahub.TrainingPortal.Application.Interfaces;

public interface ITrainingMaterialService
{
    Task<IReadOnlyList<TrainingMaterialDto>> GetForTrainingAsync(Guid trainingId);
    Task<TrainingMaterialDto> AddFileGroupAsync(Guid trainingId, CreateFileMaterialRequest request);
    Task<TrainingMaterialDto> AddTextAsync(Guid trainingId, CreateTextMaterialRequest request);
    Task<TrainingMaterialDto> AddLinkAsync(Guid trainingId, CreateLinkMaterialRequest request);
    Task<TrainingMaterialDto> UpdateTextAsync(Guid materialId, CreateTextMaterialRequest request);
    Task<TrainingMaterialDto> UpdateLinkAsync(Guid materialId, CreateLinkMaterialRequest request);
    Task<TrainingMaterialDto> UpdateFileTitleAsync(Guid materialId, UpdateFileMaterialRequest request);
    Task<TrainingMaterialDto> AddFileItemsAsync(Guid materialId, AddMaterialFileItemsRequest request);
    Task<TrainingMaterialDto> UpdateFileItemAsync(Guid materialId, Guid fileItemId, UpdateMaterialFileItemRequest request);
    Task DeleteFileItemAsync(Guid materialId, Guid fileItemId);
    Task<(Stream Stream, string ContentType, string FileName)> DownloadFileItemAsync(Guid materialId, Guid fileItemId);
    Task DeleteAsync(Guid materialId);
    Task ReorderAsync(Guid trainingId, ReorderMaterialsRequest request);
}
