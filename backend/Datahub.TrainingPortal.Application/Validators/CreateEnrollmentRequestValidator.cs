using Datahub.TrainingPortal.Application.DTOs.Enrollments;
using FluentValidation;

namespace Datahub.TrainingPortal.Application.Validators;

public class CreateEnrollmentRequestValidator : AbstractValidator<CreateEnrollmentRequest>
{
    public CreateEnrollmentRequestValidator()
    {
        RuleFor(x => x.TrainingId).NotEmpty();
        RuleFor(x => x.UserId).NotEmpty();
    }
}
