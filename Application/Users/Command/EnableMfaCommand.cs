using Application.Interface;
using Domain.Enumeration;
using MediatR;
using System.Text.Json.Serialization;

namespace Application.Accounts.Commands
{
    public class EnableMfaHandler : IRequestHandler<EnableMfaCommand, bool>
    {
        private readonly IIdentityService _identityService;
        public EnableMfaHandler(IIdentityService identityService)
        {
            _identityService = identityService;
        }

        public async Task<bool> Handle(EnableMfaCommand request, CancellationToken cancellationToken)
        {
            var result = await _identityService.EnableMfaAsync(request.CurrentUserId, request.DefaultMfaProvider, request.ClientUrl, cancellationToken);
            return result;
        }
    }

    public class EnableMfaCommand : IRequest<bool>
    {
        public MfaProvider DefaultMfaProvider { get; set; }
        [JsonIgnore]
        public string ClientUrl { get; set; } = "a";
        [JsonIgnore]
        public string CurrentUserId { get; set; }
    }
}
