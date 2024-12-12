using Common.Resources;
using FluentValidation;

namespace Application.Accounts.Commands
{
    public class AccountLoginValidator : AbstractValidator<AccountLoginCommand>
    {
        public AccountLoginValidator()
        {
            RuleFor(r => r.Email)
                .NotEmpty()
                .WithMessage(ValidationMessage.EmailRequired);

            RuleFor(r => r.Password)
                .NotEmpty()
                .WithMessage(ValidationMessage.PasswordRequired);
        }
    }
}
