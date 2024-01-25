using ChatApplication.Shared.Models.Requests;
using FluentValidation;

namespace ChatApplication.BusinessLayer.Validations;

public class LoginRequestValidator : AbstractValidator<LoginRequest>
{
    public LoginRequestValidator()
    {
        RuleFor(l => l.UserName)
            .MaximumLength(256)
            .NotEmpty()
            .WithMessage("the username is required");

        RuleFor(l => l.Password)
            .MaximumLength(256)
            .NotEmpty()
            .WithMessage("the password is required");
    }
}