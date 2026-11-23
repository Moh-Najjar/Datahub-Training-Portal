namespace Datahub.TrainingPortal.Application.DTOs.Assignments;

public class AssignmentAttachmentDto
{
    public Guid Id { get; set; }
    public string FileName { get; set; } = string.Empty;
    public string ContentType { get; set; } = string.Empty;
    public long FileSizeBytes { get; set; }
}
