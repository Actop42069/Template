using Application.Interface;
using Domain.Entities;
using Domain.Enumeration;
using MediatR;
using System.Text.Json.Serialization;

namespace Application.Accounts.Commands
{
    public class AccountSignupHandler : IRequestHandler<AccountSignupCommand, Unit>
    {
        private readonly IIdentityService _identityService;
        private readonly IErrorLogService _errorLogService;
        public AccountSignupHandler(IIdentityService identityService,
                                    IErrorLogService errorLogService)
        {
            _identityService = identityService;
            _errorLogService = errorLogService;
        }

        public async Task<Unit> Handle(AccountSignupCommand request, CancellationToken cancellationToken)
        {
            try
            {
                var roleName = "User";

                // TODO
                var rand = Guid.NewGuid().ToString().Split('-')[0];
                request.Username = request.Email.Split('@')[0] + rand;

                var dbRole = await _identityService.GetRoleByNameAsync(roleName);
                var dbUser = new User
                {
                    FirstName = request.FirstName,
                    LastName = request.LastName,
                    Email = request.Email,
                    PhoneNumber = request.PhoneNumber,
                    UserName = request.Username,
                    IsActive = false,
                    ClientUrl = request.ClientUrl,
                    Activity = ActivityLog.Created
                };
                await _identityService.CreateAsync(dbUser, dbRole.Id);

                return Unit.Value;
            }
            catch (Exception ex)
            {
                _errorLogService.LogError(ex);  
                throw;
            }
        }
    }

    public class AccountSignupCommand : IRequest<Unit>
    {
        public string FirstName { get; set; }
        public string LastName { get; set; }
        public string Email { get; set; }
        public string Username { get; set; }
        public string PhoneNumber { get; set; }
        [JsonIgnore]
        public string ClientUrl { get; set; } = "a";
    }
}
