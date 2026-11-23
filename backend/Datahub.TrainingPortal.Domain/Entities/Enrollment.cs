using Datahub.TrainingPortal.Domain.Common;
using Datahub.TrainingPortal.Domain.Enums;

namespace Datahub.TrainingPortal.Domain.Entities;

public class Enrollment : AuditableEntity
{
    public Guid TrainingId { get; set; }
    public Training? Training { get; set; }

    public Guid UserId { get; set; }
    public User? User { get; set; }

    public Guid AssignedByUserId { get; set; }
    public User? AssignedByUser { get; set; }

    public EnrollmentStatus Status { get; set; } = EnrollmentStatus.Assigned;
    public DateTime AssignedAt { get; set; } = DateTime.UtcNow;
    public DateTime? CompletedAt { get; set; }
}
