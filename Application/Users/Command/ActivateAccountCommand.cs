using Application.Interface;
using Domain.Enumeration;
using MediatR;
using System.Text.Json.Serialization;

namespace Application.Accounts.Commands
{
    public class ActivateAccountHandler : IRequestHandler<ActivateAccountCommand, bool>
    {
        private readonly IIdentityService _identityService;
        private readonly IErrorLogService _errorLogService;
        public ActivateAccountHandler(IIdentityService identityService,
                                       IErrorLogService errorLogService)
        {
            _identityService = identityService;
            _errorLogService = errorLogService;
        }

        public async Task<bool> Handle(ActivateAccountCommand request, CancellationToken cancellationToken)
        {
            try
            {
                await _identityService.ActivateAsync(request.Email, request.Token, request.Password);

                var dbUser = await _identityService.GetByEmailAsync(request.Email);
                dbUser.IsActive = true;
                dbUser.LastUpdatedAt = DateTimeOffset.UtcNow;
                dbUser.LastUpdatedBy = $"{dbUser.FirstName} {dbUser.LastName}";
                dbUser.Activity = ActivityLog.Activated;
                dbUser.ClientUrl = request.ClientUrl;

                await _identityService.UpdateAsync(dbUser);

                return true;
            }
            catch (Exception ex)
            {
                _errorLogService.LogError(ex);
                throw;
            }
        }
    }

    public class ActivateAccountCommand : IRequest<bool>
    {
        public string Email { get; set; }
        public string Password { get; set; }
        public string Token { get; set; }
        [JsonIgnore]
        public string ClientUrl { get; set; } = "a";
    }
}
