using Datahub.TrainingPortal.Domain.Common;

namespace Datahub.TrainingPortal.Domain.Entities;

public class AssignmentSubmission : AuditableEntity
{
    public Guid AssignmentId { get; set; }
    public Assignment? Assignment { get; set; }

    public Guid UserId { get; set; }
    public User? User { get; set; }

    public string TextResponse { get; set; } = string.Empty;
    public DateTime SubmittedAt { get; set; }

    public ICollection<AssignmentSubmissionAttachment> Attachments { get; set; } = new List<AssignmentSubmissionAttachment>();
}
