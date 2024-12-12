using FluentValidation;

namespace Application.Accounts.Commands
{
    public class ResendMfaTokenValidator : AbstractValidator<ResendMfaTokenCommand>
    {
        public ResendMfaTokenValidator()
        {
            RuleFor(r => r.MfaProvider)
                .IsInEnum()
                .WithMessage("Mfa provider is invalid.");

            RuleFor(r => r.ClientUrl)
                .NotEmpty()
                .WithMessage("Client url is required.");
        }
    }
}
