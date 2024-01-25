using ChatApplication.Shared.Models.Requests;
using FluentValidation;

namespace ChatApplication.BusinessLayer.Validations;

public class RegisterRequestValidator : AbstractValidator<RegisterRequest>
{
    public RegisterRequestValidator()
    {
        RuleFor(r => r.FirstName)
            .MaximumLength(256)
            .NotEmpty()
            .WithMessage("The first name is required");

        RuleFor(r => r.LastName)
            .MaximumLength(256);

        RuleFor(r => r.Email)
            .EmailAddress()
            .NotEmpty()
            .WithMessage("the email address is required");

        RuleFor(r => r.UserName)
            .MaximumLength(256)
            .NotEmpty()
            .WithMessage("the username is required");

        RuleFor(r => r.Password)
            .MaximumLength(50)
            .NotEmpty()
            .WithMessage("the password is required");
    }
}