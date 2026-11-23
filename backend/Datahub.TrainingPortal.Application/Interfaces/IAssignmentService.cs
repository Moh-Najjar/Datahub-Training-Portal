using Datahub.TrainingPortal.Application.DTOs.Assignments;
using Microsoft.AspNetCore.Http;

namespace Datahub.TrainingPortal.Application.Interfaces;

public interface IAssignmentService
{
    Task<IReadOnlyList<AssignmentDto>> GetForTrainingAsync(Guid trainingId);
    Task<AssignmentDto> CreateAsync(Guid trainingId, CreateAssignmentRequest request);
    Task<AssignmentDto> UpdateAsync(Guid assignmentId, UpdateAssignmentRequest request);
    Task DeleteAsync(Guid assignmentId);
    Task<AssignmentDto> AddAttachmentsAsync(Guid assignmentId, List<IFormFile> files);
    Task DeleteAttachmentAsync(Guid assignmentId, Guid attachmentId);
    Task<(Stream Stream, string ContentType, string FileName)> DownloadAttachmentAsync(Guid assignmentId, Guid attachmentId);
}
