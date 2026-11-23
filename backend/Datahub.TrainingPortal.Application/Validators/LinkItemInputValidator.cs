using Datahub.TrainingPortal.Application.DTOs.Materials;
using FluentValidation;

namespace Datahub.TrainingPortal.Application.Validators;

public class LinkItemInputValidator : AbstractValidator<LinkItemInput>
{
    public LinkItemInputValidator()
    {
        RuleFor(x => x.Title).MaximumLength(200);
        RuleFor(x => x.Url)
            .NotEmpty()
            .MaximumLength(2048)
            .Must(url => Uri.TryCreate(url, UriKind.Absolute, out _))
            .WithMessage("Url must be a valid absolute URL.");
    }
}
