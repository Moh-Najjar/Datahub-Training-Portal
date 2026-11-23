using Datahub.TrainingPortal.Domain.Common;

namespace Datahub.TrainingPortal.Domain.Entities;

public class Evaluation : AuditableEntity
{
    public Guid UserId { get; set; }
    public User? User { get; set; }

    public Guid EvaluatedByUserId { get; set; }
    public User? EvaluatedByUser { get; set; }

    public Guid? EnrollmentId { get; set; }
    public Enrollment? Enrollment { get; set; }

    public DateTime EvaluatedAt { get; set; }

    public ICollection<EvaluationCategoryScore> Categories { get; set; } = new List<EvaluationCategoryScore>();
}
