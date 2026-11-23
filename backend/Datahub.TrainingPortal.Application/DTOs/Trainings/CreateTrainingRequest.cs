namespace Datahub.TrainingPortal.Application.DTOs.Trainings;

public class CreateTrainingRequest
{
    public string Title { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public Guid DepartmentId { get; set; }
}
