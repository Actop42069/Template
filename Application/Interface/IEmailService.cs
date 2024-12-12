using Common.Models;

namespace Application.Interface
{
    public interface IEmailService
    {
        Task SendAsync(EmailModel email, CancellationToken cancellationToken);
    }
}
