using Datahub.TrainingPortal.Application.DTOs.Evaluations;
using FluentValidation;

namespace Datahub.TrainingPortal.Application.Validators;

public class CreateEvaluationRequestValidator : AbstractValidator<CreateEvaluationRequest>
{
    public CreateEvaluationRequestValidator()
    {
        RuleFor(x => x.EvaluatedAt).NotEqual(default(DateTime));
        RuleFor(x => x.Categories).NotEmpty();
        RuleForEach(x => x.Categories).SetValidator(new EvaluationCategoryScoreInputValidator());
    }
}
