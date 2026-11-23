using Datahub.TrainingPortal.Application.DTOs.Users;
using Datahub.TrainingPortal.Domain.Constants;
using FluentValidation;

namespace Datahub.TrainingPortal.Application.Validators;

public class UpdateUserRequestValidator : AbstractValidator<UpdateUserRequest>
{
    public UpdateUserRequestValidator()
    {
        RuleFor(x => x.FullName).NotEmpty().MaximumLength(200);
        RuleFor(x => x.Email).NotEmpty().EmailAddress();
        RuleFor(x => x.Role).NotEmpty().Must(role => RoleNames.All.Contains(role))
            .WithMessage($"Role must be one of: {string.Join(", ", RoleNames.All)}");
    }
}
