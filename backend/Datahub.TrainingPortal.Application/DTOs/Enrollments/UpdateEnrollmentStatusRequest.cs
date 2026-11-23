using Datahub.TrainingPortal.Domain.Enums;

namespace Datahub.TrainingPortal.Application.DTOs.Enrollments;

public class UpdateEnrollmentStatusRequest
{
    public EnrollmentStatus Status { get; set; }
}
