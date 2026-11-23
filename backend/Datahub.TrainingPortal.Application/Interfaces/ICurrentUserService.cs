namespace Datahub.TrainingPortal.Application.Interfaces;

public interface ICurrentUserService
{
    Guid? UserId { get; }
    string? Role { get; }
    Guid? DepartmentId { get; }
}
