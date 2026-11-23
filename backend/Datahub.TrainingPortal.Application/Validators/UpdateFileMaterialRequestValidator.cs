using Datahub.TrainingPortal.Application.DTOs.Materials;
using FluentValidation;

namespace Datahub.TrainingPortal.Application.Validators;

public class UpdateFileMaterialRequestValidator : AbstractValidator<UpdateFileMaterialRequest>
{
    public UpdateFileMaterialRequestValidator()
    {
        RuleFor(x => x.Title).NotEmpty().MaximumLength(200);
    }
}
