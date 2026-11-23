using Datahub.TrainingPortal.Application.DTOs.Departments;
using FluentValidation;

namespace Datahub.TrainingPortal.Application.Validators;

public class CreateDepartmentRequestValidator : AbstractValidator<CreateDepartmentRequest>
{
    public CreateDepartmentRequestValidator()
    {
        RuleFor(x => x.Name).NotEmpty().MaximumLength(150);
    }
}
