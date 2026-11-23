using Datahub.TrainingPortal.Application.DTOs.Enrollments;
using FluentValidation;

namespace Datahub.TrainingPortal.Application.Validators;

public class UpdateEnrollmentStatusRequestValidator : AbstractValidator<UpdateEnrollmentStatusRequest>
{
    public UpdateEnrollmentStatusRequestValidator()
    {
        RuleFor(x => x.Status).IsInEnum();
    }
}
