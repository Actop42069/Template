using Common.Resources;
using FluentValidation;

namespace Application.Accounts.Commands
{
    public class AccountSignupValidator : AbstractValidator<AccountSignupCommand>
    {
        public AccountSignupValidator()
        {
            RuleFor(r => r.FirstName)
                 .NotEmpty()
                 .WithMessage(ValidationMessage.FirstNameRequired)
                 .MaximumLength(100)
                 .WithMessage(string.Format(ValidationMessage.MaxCharacter, 100));

            RuleFor(r => r.LastName)
                .NotEmpty()
                .WithMessage(ValidationMessage.LastNameRequired)
                .MaximumLength(100)
                .WithMessage(string.Format(ValidationMessage.MaxCharacter, 100));

            RuleFor(r => r.Email)
                .MaximumLength(100)
                .WithMessage(string.Format(ValidationMessage.MaxCharacter, 100))
                .Matches(@"^[^@\s]+@[^@\s]+\.[^@\s]+$")
                .WithMessage(ValidationMessage.InvalidEmail)
                .When(w => !string.IsNullOrEmpty(w.Email))
                .NotEmpty()
                .WithMessage(ValidationMessage.EmailRequired);

            RuleFor(r => r.PhoneNumber)
                .MaximumLength(11)
                .WithMessage(string.Format(ValidationMessage.MaxCharacter, 10))
                .Matches("^[0-9]+$")
                .WithMessage(ValidationMessage.InvalidPhonenumber)
                .When(w => !string.IsNullOrEmpty(w.PhoneNumber))
                .NotEmpty()
                .WithMessage(ValidationMessage.PhonenumberRequired);

            //RuleFor(r => r.Username)
            //    .MinimumLength(3)
            //    .WithMessage("Minimum character requirement is 3.")
            //    .MaximumLength(100)
            //    .WithMessage("Maximum character limit is 100.")
            //    .Matches(@"^[a-zA-Z][\w\s]*") // Username must start with alphabet and then can have alphanumeric character
            //    .WithMessage("Invalid userame. Must start with alphabet and can only have alphanumeric.")
            //    .When(w => !string.IsNullOrEmpty(w.Username))
            //    .NotEmpty()
            //    .WithMessage("Username is required.");
        }
    }
}
