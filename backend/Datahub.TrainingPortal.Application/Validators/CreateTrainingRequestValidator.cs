using Datahub.TrainingPortal.Application.DTOs.Trainings;
using FluentValidation;

namespace Datahub.TrainingPortal.Application.Validators;

public class CreateTrainingRequestValidator : AbstractValidator<CreateTrainingRequest>
{
    public CreateTrainingRequestValidator()
    {
        RuleFor(x => x.Title).NotEmpty().MaximumLength(200);
        RuleFor(x => x.Description).NotEmpty();
        RuleFor(x => x.DepartmentId).NotEmpty();
    }
}
