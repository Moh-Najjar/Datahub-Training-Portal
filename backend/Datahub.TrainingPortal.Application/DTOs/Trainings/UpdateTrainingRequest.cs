using Datahub.TrainingPortal.Domain.Enums;

namespace Datahub.TrainingPortal.Application.DTOs.Trainings;

public class UpdateTrainingRequest
{
    public string Title { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public TrainingStatus Status { get; set; }
}
