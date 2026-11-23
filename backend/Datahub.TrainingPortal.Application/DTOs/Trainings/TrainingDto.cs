using Datahub.TrainingPortal.Domain.Enums;

namespace Datahub.TrainingPortal.Application.DTOs.Trainings;

public class TrainingDto
{
    public Guid Id { get; set; }
    public string Title { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public TrainingStatus Status { get; set; }
    public int SortOrder { get; set; }
    public Guid DepartmentId { get; set; }
    public string? DepartmentName { get; set; }
    public Guid? TrainerId { get; set; }
    public string? TrainerName { get; set; }
    public int MaterialCount { get; set; }
    public int EnrollmentCount { get; set; }
    public DateTime CreatedAt { get; set; }
}
