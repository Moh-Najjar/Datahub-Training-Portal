using Datahub.TrainingPortal.Application.DTOs.Trainings;
using FluentValidation;

namespace Datahub.TrainingPortal.Application.Validators;

public class UpdateTrainingRequestValidator : AbstractValidator<UpdateTrainingRequest>
{
    public UpdateTrainingRequestValidator()
    {
        RuleFor(x => x.Title).NotEmpty().MaximumLength(200);
        RuleFor(x => x.Description).NotEmpty();
        RuleFor(x => x.Status).IsInEnum();
    }
}
