namespace Datahub.TrainingPortal.Application.DTOs.Trainings;

public class ReorderTrainingsRequest
{
    public List<Guid> OrderedIds { get; set; } = [];
}
