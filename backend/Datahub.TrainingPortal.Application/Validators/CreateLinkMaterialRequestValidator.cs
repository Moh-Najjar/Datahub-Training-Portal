using Datahub.TrainingPortal.Application.DTOs.Materials;
using FluentValidation;

namespace Datahub.TrainingPortal.Application.Validators;

public class CreateLinkMaterialRequestValidator : AbstractValidator<CreateLinkMaterialRequest>
{
    public CreateLinkMaterialRequestValidator()
    {
        RuleFor(x => x.Title).NotEmpty().MaximumLength(200);
        RuleFor(x => x.Links).NotEmpty();
        RuleForEach(x => x.Links).SetValidator(new LinkItemInputValidator());
    }
}
