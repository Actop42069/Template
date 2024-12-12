using Application.Interface;
using MediatR;
using System.Text.Json.Serialization;

namespace Application.Accounts.Commands
{
    public class DisableMfaHandler : IRequestHandler<DisableMfaCommand, bool>
    {
        private readonly IIdentityService _identityService;
        private readonly IErrorLogService _errorLogService;
        public DisableMfaHandler(IIdentityService identityService,
                                IErrorLogService errorLogService)
        {
            _identityService = identityService;
            _errorLogService = errorLogService;
        }

        public async Task<bool> Handle(DisableMfaCommand request, CancellationToken cancellationToken)
        {
            try
            {
                var result = await _identityService.DisableMfaAsync(request.CurrentUserId, request.ClientUrl, cancellationToken);
                return result;
            }
            catch (Exception ex)
            {
                _errorLogService.LogError(ex);
                throw;
            }
        }
    }

    public class DisableMfaCommand : IRequest<bool>
    {
        [JsonIgnore]
        public string ClientUrl { get; set; } = "a";
        [JsonIgnore]
        public string CurrentUserId { get; set; }
    }
}
