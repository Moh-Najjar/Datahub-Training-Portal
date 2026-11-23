using Datahub.TrainingPortal.Application.DTOs.Assignments;

namespace Datahub.TrainingPortal.Application.Interfaces;

public interface IAssignmentSubmissionService
{
    Task<AssignmentSubmissionDto?> GetMineAsync(Guid trainingId, Guid assignmentId);
    Task<IReadOnlyList<AssignmentSubmissionOverviewDto>> GetAllForAssignmentAsync(Guid trainingId, Guid assignmentId);
    Task<AssignmentSubmissionDto> UpsertMineAsync(Guid trainingId, Guid assignmentId, UpsertAssignmentSubmissionRequest request);
    Task DeleteAttachmentAsync(Guid trainingId, Guid assignmentId, Guid attachmentId);
    Task<(Stream Stream, string ContentType, string FileName)> DownloadAttachmentAsync(Guid trainingId, Guid assignmentId, Guid attachmentId);
}
