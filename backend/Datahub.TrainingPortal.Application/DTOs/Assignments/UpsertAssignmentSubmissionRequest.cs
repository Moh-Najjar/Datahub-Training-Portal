using Microsoft.AspNetCore.Http;

namespace Datahub.TrainingPortal.Application.DTOs.Assignments;

public class UpsertAssignmentSubmissionRequest
{
    public string TextResponse { get; set; } = string.Empty;
    public List<IFormFile>? Attachments { get; set; }
}
