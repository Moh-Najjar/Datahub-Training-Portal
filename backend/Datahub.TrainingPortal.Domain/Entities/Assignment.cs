using Datahub.TrainingPortal.Domain.Common;

namespace Datahub.TrainingPortal.Domain.Entities;

public class Assignment : AuditableEntity
{
    public Guid TrainingId { get; set; }
    public Training? Training { get; set; }

    public string Title { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;

    public DateTime OpensAt { get; set; }
    public DateTime ClosesAt { get; set; }

    public Guid CreatedByUserId { get; set; }
    public User? CreatedByUser { get; set; }

    public ICollection<AssignmentAttachment> Attachments { get; set; } = new List<AssignmentAttachment>();
    public ICollection<AssignmentSubmission> Submissions { get; set; } = new List<AssignmentSubmission>();
}
