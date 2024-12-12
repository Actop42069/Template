using Application.Interface;
using Common.Exceptions;
using Common.Resources;
using Domain.Enumeration;
using MediatR;
using System.Text.Json.Serialization;

namespace Application.Accounts.Commands
{
    public class ResetAccountPasswordHandler : IRequestHandler<ResetAccountPasswordCommand, Unit>
    {
        private readonly IIdentityService _identityService;
        private readonly IErrorLogService _errorLogService;
        public ResetAccountPasswordHandler(IIdentityService identityService,
                                            IErrorLogService errorLogService)
        {
            _identityService = identityService;
            _errorLogService = errorLogService;
        }

        public async Task<Unit> Handle(ResetAccountPasswordCommand request, CancellationToken cancellationToken)
        {
            try
            {
                var dbUser = await _identityService.GetByEmailAsync(request.Email);
                if (dbUser == null)
                {
                    throw new BadRequestException(ExceptionMessage.InvalidUserRequest);
                }

                await _identityService.ResetPasswordAsync(dbUser, request.Token, request.Password);

                dbUser.LastUpdatedAt = DateTimeOffset.UtcNow;
                dbUser.LastUpdatedBy = $"{dbUser.FirstName} {dbUser.LastName}";
                dbUser.Activity = ActivityLog.PasswordReset;
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

    public class ResetAccountPasswordCommand : IRequest<Unit>
    {
        public string Email { get; set; }
        public string Token { get; set; }
        public string Password { get; set; }
        [JsonIgnore]
        public string ClientUrl { get; set; } = "a";
    }
}
