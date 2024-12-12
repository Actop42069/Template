using Application.Interface;
using Common.Exceptions;
using Common.Resources;
using Domain.Enumeration;
using MediatR;
using System.Text.Json.Serialization;

namespace Application.AdminArea.Accounts.Commands
{
    public class RequestResetAccountPasswordHandler : IRequestHandler<RequestResetAccountPasswordCommand, Unit>
    {
        private readonly IIdentityService _identityService;
        private readonly IErrorLogService _errorLogService;
        public RequestResetAccountPasswordHandler(IIdentityService identityService,
                                                    IErrorLogService errorLogService)
        {
            _identityService = identityService;
            _errorLogService = errorLogService;
        }

        public async Task<Unit> Handle(RequestResetAccountPasswordCommand request, CancellationToken cancellationToken)
        {
            try
            {
                var dbUser = await _identityService.GetByEmailAsync(request.Email);
                if (dbUser == null)
                {
                    throw new NotFoundException(ExceptionMessage.InvalidUserRequest);
                }

                if (!dbUser.EmailConfirmed)
                {
                    throw new BadRequestException(ExceptionMessage.ContactAdminToResetPassword);
                }

                dbUser.LastUpdatedBy = $"{dbUser.FirstName} {dbUser.LastName}";
                dbUser.LastUpdatedAt = DateTimeOffset.UtcNow;
                dbUser.Activity = ActivityLog.RequestPasswordReset;
                dbUser.ClientUrl = request.ClientUrl;

                await _identityService.UpdateAsync(dbUser);

                return Unit.Value;
            }
            catch (Exception ex)
            {
                _errorLogService.LogError(ex);
                throw;
            }
        }
    }

    public class RequestResetAccountPasswordCommand : IRequest<Unit>
    {
        public string Email { get; set; }
        [JsonIgnore]
        public string ClientUrl { get; set; } = "a";
    }
}
