using Datahub.TrainingPortal.Application.DTOs.Users;
using Datahub.TrainingPortal.Domain.Constants;
using FluentValidation;

namespace Datahub.TrainingPortal.Application.Validators;

public class CreateUserRequestValidator : AbstractValidator<CreateUserRequest>
{
    public CreateUserRequestValidator()
    {
        RuleFor(x => x.FullName).NotEmpty().MaximumLength(200);
        RuleFor(x => x.Email).NotEmpty().EmailAddress();
        RuleFor(x => x.Password).NotEmpty().MinimumLength(8);
        RuleFor(x => x.Role).NotEmpty().Must(role => RoleNames.All.Contains(role))
            .WithMessage($"Role must be one of: {string.Join(", ", RoleNames.All)}");
    }
}
