namespace Datahub.TrainingPortal.Application.DTOs.Enrollments;

public class CreateEnrollmentRequest
{
    public Guid TrainingId { get; set; }
    public Guid UserId { get; set; }
}
