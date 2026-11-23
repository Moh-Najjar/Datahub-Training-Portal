using Datahub.TrainingPortal.Domain.Enums;

namespace Datahub.TrainingPortal.Application.DTOs.Enrollments;

public class EnrollmentDto
{
    public Guid Id { get; set; }
    public Guid TrainingId { get; set; }
    public string? TrainingTitle { get; set; }
    public Guid UserId { get; set; }
    public string? UserName { get; set; }
    public Guid AssignedByUserId { get; set; }
    public string? AssignedByUserName { get; set; }
    public EnrollmentStatus Status { get; set; }
    public DateTime AssignedAt { get; set; }
    public DateTime? CompletedAt { get; set; }
}
