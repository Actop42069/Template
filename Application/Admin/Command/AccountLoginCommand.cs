using Application.Interface;
using Common.Exceptions;
using Domain.Entities;
using Domain.Enumeration;
using MediatR;

namespace Application.AdminArea.Accounts.Commands
{
    public class AccountLoginCommand : IRequest<AccountLoginResponse>
    {
        public string Email { get; set; }
        public string Password { get; set; }
    }

    public class AccountLoginResponse
    {
        public string FirstName { get; set; }
        public string LastName { get; set; }
        public AuthTokenType TokenType { get; set; }
        public string Role { get; set; }
        public string Token { get; set; }
    }

    public class AccountLoginHandler : IRequestHandler<AccountLoginCommand, AccountLoginResponse>
    {
        private readonly IIdentityService _identityService;
        private readonly IErrorLogService _errorLogService;

        public AccountLoginHandler(
            IIdentityService identityService,
            IErrorLogService errorLogService)
        {
            _identityService = identityService;
            _errorLogService = errorLogService;
        }

        public async Task<AccountLoginResponse> Handle(AccountLoginCommand request, CancellationToken cancellationToken)
        {
            try
            {
                User dbUser = await _identityService.GetByEmailAsync(request.Email);
                var validRoles = new string[] { "Super Admin", "Admin" };

                if (!dbUser.UserRoles.Select(s => s.Role.Name).Any(a => validRoles.Contains(a)))
                    throw new BadRequestException("User does not have a valid role");

                var authResult = await _identityService.AuthenticateAsync(dbUser.Email, request.Password, cancellationToken);

                return new AccountLoginResponse
                {
                    FirstName = dbUser.FirstName,
                    LastName = dbUser.LastName,
                    TokenType = authResult.TokenType,
                    Role = dbUser.UserRoles.First().Role.Name,
                    Token = authResult.Token
                };
            }
            catch (Exception ex)
            {
                await _errorLogService.LogErrorAsync(ex);
                throw;
            }
        }
    }
}