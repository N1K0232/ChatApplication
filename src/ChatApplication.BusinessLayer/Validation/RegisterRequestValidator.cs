using ChatApplication.Shared.Models.Requests;
using FluentValidation;

namespace ChatApplication.BusinessLayer.Validation;

public class RegisterRequestValidator : AbstractValidator<RegisterRequest>
{
    public RegisterRequestValidator()
    {
        RuleFor(user => user.FirstName)
            .NotEmpty()
            .WithMessage("the first name is required");

        RuleFor(user => user.LastName)
            .NotEmpty()
            .WithMessage("the last name is required");

        RuleFor(user => user.UserName)
            .NotEmpty()
            .WithMessage("the user name is required");

        RuleFor(user => user.Email)
            .EmailAddress()
            .NotEmpty()
            .WithMessage("the email is required");

        RuleFor(user => user.Password)
            .NotEmpty()
            .WithMessage("the password is required");
    }
}