using Datahub.TrainingPortal.Domain.Common;
using Datahub.TrainingPortal.Domain.Enums;

namespace Datahub.TrainingPortal.Domain.Entities;

public class Training : AuditableEntity
{
    public string Title { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public TrainingStatus Status { get; set; } = TrainingStatus.Draft;
    public int SortOrder { get; set; }

    public Guid DepartmentId { get; set; }
    public Department? Department { get; set; }

    public Guid? TrainerId { get; set; }
    public User? Trainer { get; set; }

    public ICollection<TrainingMaterial> Materials { get; set; } = new List<TrainingMaterial>();
    public ICollection<Enrollment> Enrollments { get; set; } = new List<Enrollment>();
    public ICollection<Assignment> Assignments { get; set; } = new List<Assignment>();
}
