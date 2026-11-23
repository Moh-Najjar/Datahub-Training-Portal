using Datahub.TrainingPortal.Application.DTOs.Trainings;

namespace Datahub.TrainingPortal.Application.Interfaces;

public interface ITrainingService
{
    Task<IReadOnlyList<TrainingDto>> GetAllAsync(Guid? departmentId = null);
    Task<TrainingDto> GetByIdAsync(Guid id);
    Task<TrainingDto> CreateAsync(CreateTrainingRequest request);
    Task<TrainingDto> UpdateAsync(Guid id, UpdateTrainingRequest request);
    Task DeleteAsync(Guid id);
    Task ReorderAsync(ReorderTrainingsRequest request);
}
