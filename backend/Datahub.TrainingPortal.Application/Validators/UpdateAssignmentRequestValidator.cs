using Datahub.TrainingPortal.Application.DTOs.Assignments;
using FluentValidation;

namespace Datahub.TrainingPortal.Application.Validators;

public class UpdateAssignmentRequestValidator : AbstractValidator<UpdateAssignmentRequest>
{
    public UpdateAssignmentRequestValidator()
    {
        RuleFor(x => x.Title).NotEmpty().MaximumLength(200);
        RuleFor(x => x.Description).NotEmpty();
        RuleFor(x => x.ClosesAt)
            .GreaterThan(x => x.OpensAt)
            .WithMessage("The close time must be after the open time.");
    }
}
