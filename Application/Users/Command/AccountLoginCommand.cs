using Application.Interface;
using Common.Exceptions;
using Domain.Enumeration;
using MediatR;

namespace Application.Accounts.Commands
{
    public class AccountLoginHandler : IRequestHandler<AccountLoginCommand, AccountLoginResponse>
    {
        private readonly IIdentityService _identityService;
        private readonly IErrorLogService _errorLogService;
        public AccountLoginHandler(IIdentityService identityService,
                                    IErrorLogService errorLogService)
        {
            _identityService = identityService;
            _errorLogService = errorLogService;
        }

        public async Task<AccountLoginResponse> Handle(AccountLoginCommand request, CancellationToken cancellationToken)
        {
            try
            {
                var dbUser = await _identityService.GetByEmailAsync(request.Email);
                var validRoles = new string[] { "User" };
                if (!dbUser.UserRoles.Select(s => s.Role.Name).Any(a => validRoles.Contains(a)))
                    throw new BadRequestException();

                var authResult = await _identityService.AuthenticateAsync(dbUser.Email, request.Password, cancellationToken);

                var response = new AccountLoginResponse
                {
                    FirstName = dbUser.FirstName,
                    LastName = dbUser.LastName,
                    TokenType = authResult.TokenType,
                    Token = authResult.Token
                };
                return response;
            }
            catch (Exception ex)
            {
                _errorLogService.LogError(ex);
                throw;
            }
        }
    }

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
        public string Token { get; set; }
    }
}
