using Application.Interface;
using MediatR;
using System.Text.Json.Serialization;

namespace Application.Users.Query
{
    public class ListUsersHandler : IRequestHandler<ListUsersQuery, List<ListUsersResponse>>
    {
        private readonly ITemplateDbContext _dbContext;
        private readonly IIdentityService _identityService;
        private readonly IErrorLogService _errorLogService;
        public ListUsersHandler(ITemplateDbContext dbContext,
                                IIdentityService identityService,
                                IErrorLogService errorLogService)
        {
            _dbContext = dbContext;
            _identityService = identityService;
            _errorLogService = errorLogService;
        }

        public async Task<List<ListUsersResponse>> Handle(ListUsersQuery request, CancellationToken cancellationToken)
        {
            try
            {
                var dbUsers = await _identityService.ListUsersAsync(request.Role, cancellationToken);
                var response = dbUsers.Select(s => new ListUsersResponse
                {
                    Id = s.Id,
                    FirstName = s.FirstName,
                    LastName = s.LastName,
                    Email = s.Email,
                    PhoneNumber = s.PhoneNumber,
                    Username = s.UserName,
                    IsEmailConfirmed = s.EmailConfirmed
                })
                .OrderBy(o => o.FirstName)
                .ThenBy(o => o.LastName)
                .ToList();

                return response;
            }
            catch (Exception ex)
            {
                _errorLogService.LogError(ex);
                throw;
            }
        }
    }

    public class ListUsersQuery : IRequest<List<ListUsersResponse>>
    {
        [JsonIgnore]
        public string Role { get; set; }
    }

    public class ListUsersResponse
    {
        public string Id { get; set; }
        public string FirstName { get; set; }
        public string LastName { get; set; }
        public string Email { get; set; }
        public string PhoneNumber { get; set; }
        public string Username { get; set; }
        public bool IsEmailConfirmed { get; set; }
    }
}