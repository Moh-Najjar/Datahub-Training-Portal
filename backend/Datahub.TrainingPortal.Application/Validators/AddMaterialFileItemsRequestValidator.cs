using Datahub.TrainingPortal.Application.DTOs.Materials;
using FluentValidation;

namespace Datahub.TrainingPortal.Application.Validators;

public class AddMaterialFileItemsRequestValidator : AbstractValidator<AddMaterialFileItemsRequest>
{
    public AddMaterialFileItemsRequestValidator()
    {
        RuleFor(x => x.Files).NotEmpty().WithMessage("At least one file is required.");
    }
}
