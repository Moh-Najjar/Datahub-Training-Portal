using Datahub.TrainingPortal.Application.DTOs.Departments;
using FluentValidation;

namespace Datahub.TrainingPortal.Application.Validators;

public class UpdateDepartmentRequestValidator : AbstractValidator<UpdateDepartmentRequest>
{
    public UpdateDepartmentRequestValidator()
    {
        RuleFor(x => x.Name).NotEmpty().MaximumLength(150);
    }
}
