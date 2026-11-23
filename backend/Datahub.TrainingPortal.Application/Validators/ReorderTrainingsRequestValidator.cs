using Datahub.TrainingPortal.Application.DTOs.Trainings;
using FluentValidation;

namespace Datahub.TrainingPortal.Application.Validators;

public class ReorderTrainingsRequestValidator : AbstractValidator<ReorderTrainingsRequest>
{
    public ReorderTrainingsRequestValidator()
    {
        RuleFor(x => x.OrderedIds).NotEmpty();
    }
}
