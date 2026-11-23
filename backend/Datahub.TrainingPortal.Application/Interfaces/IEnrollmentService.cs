using Datahub.TrainingPortal.Application.DTOs.Enrollments;

namespace Datahub.TrainingPortal.Application.Interfaces;

public interface IEnrollmentService
{
    Task<IReadOnlyList<EnrollmentDto>> GetAllAsync(Guid? trainingId = null, Guid? userId = null);
    Task<EnrollmentDto> GetByIdAsync(Guid id);
    Task<EnrollmentDto> CreateAsync(CreateEnrollmentRequest request);
    Task<EnrollmentDto> UpdateStatusAsync(Guid id, UpdateEnrollmentStatusRequest request);
    Task DeleteAsync(Guid id);
}
