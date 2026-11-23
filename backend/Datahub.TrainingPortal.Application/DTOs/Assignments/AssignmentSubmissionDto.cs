namespace Datahub.TrainingPortal.Application.DTOs.Assignments;

public class AssignmentSubmissionDto
{
    public Guid Id { get; set; }
    public Guid AssignmentId { get; set; }
    public Guid UserId { get; set; }
    public string TextResponse { get; set; } = string.Empty;
    public DateTime SubmittedAt { get; set; }
    public DateTime? UpdatedAt { get; set; }
    public List<AssignmentAttachmentDto> Attachments { get; set; } = [];
}
