using Datahub.TrainingPortal.Application.DTOs.Materials;
using FluentValidation;

namespace Datahub.TrainingPortal.Application.Validators;

public class CreateTextMaterialRequestValidator : AbstractValidator<CreateTextMaterialRequest>
{
    public CreateTextMaterialRequestValidator()
    {
        RuleFor(x => x.Title).NotEmpty().MaximumLength(200);
        RuleFor(x => x.Content).NotEmpty();
    }
}
