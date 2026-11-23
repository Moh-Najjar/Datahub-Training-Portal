using Datahub.TrainingPortal.Application.DTOs.Materials;
using FluentValidation;

namespace Datahub.TrainingPortal.Application.Validators;

public class CreateFileMaterialRequestValidator : AbstractValidator<CreateFileMaterialRequest>
{
    public CreateFileMaterialRequestValidator()
    {
        RuleFor(x => x.Title).MaximumLength(200);
        RuleFor(x => x.Files).NotEmpty().WithMessage("At least one file is required.");
    }
}
