using Common.Resources;
using FluentValidation;

namespace Application.AdminArea.Accounts.Commands
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
