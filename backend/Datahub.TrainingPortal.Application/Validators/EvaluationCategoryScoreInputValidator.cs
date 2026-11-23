using Datahub.TrainingPortal.Application.DTOs.Evaluations;
using FluentValidation;

namespace Datahub.TrainingPortal.Application.Validators;

public class EvaluationCategoryScoreInputValidator : AbstractValidator<EvaluationCategoryScoreInput>
{
    public EvaluationCategoryScoreInputValidator()
    {
        RuleFor(x => x.CategoryName).NotEmpty().MaximumLength(200);
        RuleFor(x => x.Criteria).MaximumLength(1000);
        RuleFor(x => x.Score).InclusiveBetween(1, 10);
    }
}
