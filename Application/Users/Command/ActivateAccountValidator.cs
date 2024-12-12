using Common.Resources;
using FluentValidation;
using System.Text.RegularExpressions;

namespace Application.Accounts.Commands
{
    public class ActivateAccountValidator : AbstractValidator<ActivateAccountCommand>
    {
        public ActivateAccountValidator()
        {
            RuleFor(r => r.Email)
                .NotEmpty()
                .WithMessage(ValidationMessage.EmailRequired);

            RuleFor(r => r.Password)
                .MinimumLength(8)
                .WithMessage(string.Format(ValidationMessage.MinimumCharacter, 8))
                .MaximumLength(100)
                .WithMessage(string.Format(ValidationMessage.MaxCharacter, 100))
                .Must(password =>
                {
                    var result = Regex.IsMatch(password, "[A-Z]+");
                    return result;
                })
                .WithMessage(ValidationMessage.PasswordUpperCase)
                .Must(password =>
                {
                    var result = Regex.IsMatch(password, "[a-z]+");
                    return result;
                })
                .WithMessage(ValidationMessage.PasswordLowerCase)
                .Must(password =>
                {
                    var result = Regex.IsMatch(password, "[0-9]+");
                    return result;
                })
                .WithMessage(ValidationMessage.PasswordNumber)
                .Must(password =>
                {
                    var result = Regex.IsMatch(password, "[!@#$%^&*()_=+]+");
                    return result;
                })
                .WithMessage(ValidationMessage.PasswordSpecial)
                .When(w => !string.IsNullOrEmpty(w.Password))
                .NotEmpty()
                .WithMessage(ValidationMessage.PasswordRequired);

            RuleFor(r => r.Token)
                .NotEmpty()
                .WithMessage(ValidationMessage.TokenRequired);
        }
    }
}
