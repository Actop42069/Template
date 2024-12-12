using Common.Resources;
using FluentValidation;

namespace Application.Accounts.Commands
{
    public class RequestResetAccountPasswordValidator : AbstractValidator<RequestResetAccountPasswordCommand>
    {
        public RequestResetAccountPasswordValidator()
        {
            RuleFor(r => r.Email)
                .NotEmpty()
                .WithMessage(ValidationMessage.EmailRequired);
        }
    }
}
