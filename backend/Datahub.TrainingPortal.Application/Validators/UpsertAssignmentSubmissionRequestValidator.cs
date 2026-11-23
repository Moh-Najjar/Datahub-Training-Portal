using Datahub.TrainingPortal.Application.DTOs.Assignments;
using FluentValidation;

namespace Datahub.TrainingPortal.Application.Validators;

public class UpsertAssignmentSubmissionRequestValidator : AbstractValidator<UpsertAssignmentSubmissionRequest>
{
    public UpsertAssignmentSubmissionRequestValidator()
    {
        RuleFor(x => x.TextResponse).MaximumLength(4000);
    }
}
