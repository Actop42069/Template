using Application.Interface;
using Domain.Enumeration;
using MediatR;
using System.Text.Json.Serialization;

namespace Application.Accounts.Commands
{
    public class ResendMfaTokenHandler : IRequestHandler<ResendMfaTokenCommand, bool>
    {
        private readonly IIdentityService _identityService;
        public ResendMfaTokenHandler(IIdentityService identityService)
        {
            _identityService = identityService;
        }

        public async Task<bool> Handle(ResendMfaTokenCommand request, CancellationToken cancellationToken)
        {
            await _identityService.ResendMfaTokenAsync(request.CurrentUserId, request.MfaProvider, request.ClientUrl, cancellationToken);
            return true;
        }
    }

    public class ResendMfaTokenCommand : IRequest<bool>
    {
        public MfaProvider MfaProvider { get; set; }
        [JsonIgnore]
        public string ClientUrl { get; set; }
        [JsonIgnore]
        public string CurrentUserId { get; set; }
    }
}
