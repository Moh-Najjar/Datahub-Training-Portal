using Microsoft.AspNetCore.Http;

namespace Datahub.TrainingPortal.Application.DTOs.Assignments;

public class CreateAssignmentRequest
{
    public string Title { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public DateTime OpensAt { get; set; }
    public DateTime ClosesAt { get; set; }
    public List<IFormFile>? Attachments { get; set; }
}
