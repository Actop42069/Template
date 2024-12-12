using Application.Model;
using Domain.Entities;
using Domain.Enumeration;

namespace Application.Interface
{
    public interface IIdentityService
    {
        Task<AuthResponseModel> AuthenticateAsync(string email, string password, CancellationToken cancellationToken);
        Task<bool> ResendMfaTokenAsync(string userId, MfaProvider mfaProvider, string clientUrl, CancellationToken cancellationToken);
        Task<AuthResponseModel> VerifyTwoFactorTokenAsync(string userId, string provider, string token, CancellationToken cancellationToken);
        Task<bool> VerifyPhoneNumberAsync(string email, string token, string phoneNumber, string clientUrl, CancellationToken cancellationToken);
        Task<bool> RemovePhoneNumberAsync(string phoneNumber, string userId, string clientUrl, CancellationToken cancellationToken);
        Task<bool> EnableMfaAsync(string userId, MfaProvider defaultProvider, string clientUrl, CancellationToken cancellationToken);
        Task<bool> DisableMfaAsync(string userId, string clientUrl, CancellationToken cancellationToken);
        Task<MfaProvider[]> ListUserMfaProvidersAsync(string userId, CancellationToken cancellationToken);
        Task<bool> RequestVerificationCodeAsync(string userId, string phoneNumber, string clientUrl, CancellationToken cancellationToken);
        Task<bool> ActivateAsync(string email, string token, string password);
        Task<bool> ResetPasswordAsync(User dbUser, string token, string password);
        Task<bool> CheckPasswordAsync(User dbUser, string password);
        Task<bool> CreateAsync(User dbUser, string roleId);
        Task<bool> DeleteAsync(User dbUser);
        Task<bool> ReconfirmUserEmailAsync(User dbUser);
        Task<string> GenerateEmailConfirmationTokenAsync(User dbUser);
        Task<string> GenerateResetPasswordTokenAsync(User dbUser);
        Task<User> GetByEmailAsync(string userId);
        Task<User> GetByIdAsync(string id);
        Task<User> GetByNameAsync(string username);
        Task<Role> GetRoleByNameAsync(string roleName);
        Task<bool> UpdateAsync(User dbUser);
        Task<List<User>> ListUsersAsync(string roleName, CancellationToken cancellationToken);
    }
}
