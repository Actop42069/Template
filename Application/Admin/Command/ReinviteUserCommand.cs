using Application.Interface;
using Common.Exceptions;
using Domain.Enumeration;
using MediatR;
using System.Text.Json.Serialization;

namespace Application.AdminArea.Users.Commands
{
    public class ReinviteUserHandler : IRequestHandler<ReinviteUserCommand, Unit>
    {
        private readonly IIdentityService _identityService;
        private readonly IErrorLogService _errorLogService;
        public ReinviteUserHandler(IIdentityService identityService,
                                    IErrorLogService errorLogService)
        {
            _identityService = identityService;
            _errorLogService = errorLogService;
        }

        public async Task<Unit> Handle(ReinviteUserCommand request, CancellationToken cancellationToken)
        {
            try
            {
                var dbUser = await _identityService.GetByIdAsync(request.Id);
                if (dbUser == null)
                {
                    throw new NotFoundException("Invalid user id.");
                }

                dbUser.ClientUrl = request.ClientUrl;
                dbUser.LastUpdatedBy = request.CurrentUser;
                dbUser.Activity = ActivityLog.RequestReinvite;
                dbUser.IsActive = false;

                await _identityService.ReconfirmUserEmailAsync(dbUser);

                return Unit.Value;
            }
            catch (Exception ex)
            {
                _errorLogService.LogError(ex);
                throw;
            }
        }
    }

    public class ReinviteUserCommand : IRequest<Unit>
    {
        public string Id { get; set; }
        [JsonIgnore]
        public string CurrentUser { get; set; }
        [JsonIgnore]
        public string ClientUrl { get; set; } = "a";
    }
}
