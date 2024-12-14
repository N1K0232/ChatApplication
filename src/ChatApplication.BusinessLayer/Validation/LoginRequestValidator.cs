using ChatApplication.Shared.Models.Requests;
using FluentValidation;

namespace ChatApplication.BusinessLayer.Validation;

public class LoginRequestValidator : AbstractValidator<LoginRequest>
{
    public LoginRequestValidator()
    {
        RuleFor(l => l.Email)
            .EmailAddress()
            .NotEmpty()
            .WithMessage("The email is required");

        RuleFor(l => l.Password)
            .NotEmpty()
            .WithMessage("The password is required");
    }
}