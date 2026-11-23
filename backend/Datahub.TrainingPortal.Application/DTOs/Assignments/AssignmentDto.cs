using Datahub.TrainingPortal.Domain.Enums;

namespace Datahub.TrainingPortal.Application.DTOs.Assignments;

public class AssignmentDto
{
    public Guid Id { get; set; }
    public Guid TrainingId { get; set; }
    public string Title { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public DateTime OpensAt { get; set; }
    public DateTime ClosesAt { get; set; }
    public AssignmentStatus Status { get; set; }
    public Guid CreatedByUserId { get; set; }
    public string? CreatedByUserName { get; set; }
    public DateTime CreatedAt { get; set; }
    public List<AssignmentAttachmentDto> Attachments { get; set; } = [];
}
