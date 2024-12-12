using Application.Interface;
using Domain.Entities;
using Domain.Enumeration;
using MediatR;
using System.Text.Json.Serialization;

namespace Application.AdminArea.Users.Commands
{
    public class CreateUserHandler : IRequestHandler<CreateUserCommand, CreateUserResponse>
    {
        private readonly IIdentityService _identityService;
        private readonly IErrorLogService _errorLogService;
        public CreateUserHandler(IIdentityService identityService,
                                  IErrorLogService errorLogService)
        {
            _identityService = identityService;
            _errorLogService = errorLogService;
        }

        public async Task<CreateUserResponse> Handle(CreateUserCommand request, CancellationToken cancellationToken)
        {
            try
            {
                var roleName = "Admin";

                var dbRole = await _identityService.GetRoleByNameAsync(roleName);
                var dbUser = new User
                {
                    FirstName = request.FirstName,
                    LastName = request.LastName,
                    Email = request.Email,
                    PhoneNumber = request.PhoneNumber,
                    UserName = request.Username,
                    IsActive = false,
                    LastUpdatedBy = request.CurrentUser,
                    ClientUrl = request.ClientUrl,
                    Activity = ActivityLog.Created
                };

                await _identityService.CreateAsync(dbUser, dbRole.Id);

                return new CreateUserResponse
                {
                    Id = dbUser.Id
                };
            }
            catch (Exception ex)
            {
                await _errorLogService.LogErrorAsync(ex);
                throw;
            }
        }
    }

    public class CreateUserCommand : IRequest<CreateUserResponse>
    {
        public string FirstName { get; set; }
        public string LastName { get; set; }
        public string Email { get; set; }
        public string Username { get; set; }
        public string PhoneNumber { get; set; }
        [JsonIgnore]
        public string CurrentUser { get; set; }
        [JsonIgnore]
        public string ClientUrl { get; set; } = "a";
    }

    public class CreateUserResponse
    {
        public string Id { get; set; }
    }
}
