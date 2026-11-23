using Datahub.TrainingPortal.Application.DTOs.Materials;
using FluentValidation;

namespace Datahub.TrainingPortal.Application.Validators;

public class UpdateMaterialFileItemRequestValidator : AbstractValidator<UpdateMaterialFileItemRequest>
{
    public UpdateMaterialFileItemRequestValidator()
    {
        RuleFor(x => x.Title).MaximumLength(200);
    }
}
