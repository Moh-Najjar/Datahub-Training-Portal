using Datahub.TrainingPortal.Domain.Common;

namespace Datahub.TrainingPortal.Domain.Entities;

public class User : AuditableEntity
{
    public string FullName { get; set; } = string.Empty;
    public string Email { get; set; } = string.Empty;
    public string PasswordHash { get; set; } = string.Empty;
    public string Role { get; set; } = string.Empty;
    public bool IsActive { get; set; } = true;

    public Guid? DepartmentId { get; set; }
    public Department? Department { get; set; }

    public ICollection<Training> TrainingsAuthored { get; set; } = new List<Training>();
    public ICollection<Enrollment> Enrollments { get; set; } = new List<Enrollment>();
}
