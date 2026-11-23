using Datahub.TrainingPortal.Application.DTOs.Materials;
using FluentValidation;

namespace Datahub.TrainingPortal.Application.Validators;

public class ReorderMaterialsRequestValidator : AbstractValidator<ReorderMaterialsRequest>
{
    public ReorderMaterialsRequestValidator()
    {
        RuleFor(x => x.OrderedIds).NotEmpty();
    }
}
