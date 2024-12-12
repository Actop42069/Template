using FluentValidation;

namespace Application.AdminArea.Users.Commands
{
    public class CreateUserValidator : AbstractValidator<CreateUserCommand>
    {
        public CreateUserValidator()
        {
            RuleFor(r => r.FirstName)
                 .NotEmpty()
                 .WithMessage("First name is required.")
                 .MaximumLength(100)
                 .WithMessage("Maximum character limit is 100.");

            RuleFor(r => r.LastName)
                .NotEmpty()
                .WithMessage("Last name is required.")
                .MaximumLength(100)
                .WithMessage("Maximum character limit is 100.");

            RuleFor(r => r.Email)
                .MaximumLength(100)
                .WithMessage("Maximum character limit is 100.")
                .Matches(@"^[^@\s]+@[^@\s]+\.[^@\s]+$")
                .WithMessage("Invalid email address.")
                .When(w => !string.IsNullOrEmpty(w.Email))
                .NotEmpty()
                .WithMessage("Email is required.");

            RuleFor(r => r.PhoneNumber)
                .MaximumLength(20)
                .WithMessage("Maximum character limit is 10.")
                .Matches("^[0-9]+$")
                .WithMessage("Invalid phone number.")
                .When(w => !string.IsNullOrEmpty(w.PhoneNumber))
                .NotEmpty()
                .WithMessage("Phone number is required.");

            RuleFor(r => r.Username)
                .MinimumLength(3)
                .WithMessage("Minimum character requirement is 3.")
                .MaximumLength(100)
                .WithMessage("Maximum character limit is 100.")
                .Matches(@"^[a-zA-Z][\w\s]*") // Username must start with alphabet and then can have alphanumeric character
                .WithMessage("Invalid userame. Must start with alphabet and can only have alphanumeric.")
                .When(w => !string.IsNullOrEmpty(w.Username))
                .NotEmpty()
                .WithMessage("Username is required.");
        }
    }
}
