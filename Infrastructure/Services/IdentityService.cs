using Application.Interface;
using Application.Model;
using Common.Configurations;
using Common.Exceptions;
using Common.Resources;
using Domain.Entities;
using Domain.Enumeration;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.WebUtilities;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.Tokens;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using System.Text.Json;

namespace Infrastructure.Services
{
    public class IdentityService : IIdentityService
    {
        private readonly UserManager<User> _userManager;
        private readonly RoleManager<Role> _roleManager;
        private readonly SignInManager<User> _signInManager;
        private readonly JwtConfiguration _jwtConfig;
        public IdentityService(UserManager<User> userManager,
                               RoleManager<Role> roleManager,
                               SignInManager<User> signInManager,
                               IOptions<JwtConfiguration> jwtOptions)
        {
            _userManager = userManager;
            _roleManager = roleManager;
            _signInManager = signInManager;
            _jwtConfig = jwtOptions.Value;
        }

        #region Authentication
        public async Task<AuthResponseModel> AuthenticateAsync(string email, string password, CancellationToken cancellationToken)
        {
            var dbUser = await _userManager.Users.Include(i => i.UserRoles)
                                                 .ThenInclude(i => i.Role)
                                                 .Where(w => w.NormalizedEmail == email.ToUpper())
                                                 .FirstOrDefaultAsync(cancellationToken);
            if (dbUser == null)
                throw new BadRequestException("Invalid email or password.");

            var result = await _signInManager.PasswordSignInAsync(dbUser, password, true, true);
            if (result.IsLockedOut)
                throw new BadRequestException("Account confirmation process not completed.");
            else if (result.IsLockedOut)
                throw new BadRequestException("Account is locked.");
            else if (result.RequiresTwoFactor)
                return await GetMfaTokenAsync(dbUser, cancellationToken);
            else if (!result.Succeeded)
                throw new BadRequestException("Invalid email or password");

            var token = GenerateToken(dbUser);

            var response = new AuthResponseModel
            {
                FirstName = dbUser.FirstName,
                LastName = dbUser.LastName,
                TokenType = AuthTokenType.JWT,
                Token = token,
            };
            return response;
        }

        public async Task<bool> ResendMfaTokenAsync(string userId, MfaProvider mfaProvider, string clientUrl, CancellationToken cancellationToken)
        {
            var dbUser = await _userManager.Users.Include(i => i.UserRoles)
                                                 .ThenInclude(i => i.Role)
                                                 .Where(w => w.Id == userId)
                                                 .FirstOrDefaultAsync(cancellationToken);
            if (dbUser == null)
                throw new NotFoundException();

            if (mfaProvider == MfaProvider.Email && dbUser.EmailConfirmed == false)
                throw new BadRequestException("Invalid mfa provider.");
            if (mfaProvider == MfaProvider.Phone && dbUser.PhoneNumberConfirmed == false)
                throw new BadRequestException("Invalid mfa provider.");

            var mfaToken = await _userManager.GenerateTwoFactorTokenAsync(dbUser, $"{mfaProvider}");

            dbUser.LastUpdatedAt = DateTimeOffset.UtcNow;
            dbUser.LastUpdatedBy = userId;
            dbUser.Activity = mfaProvider == MfaProvider.Email ? ActivityLog.MfaTokenToEmail : ActivityLog.MfaTokenToPhone;
            dbUser.Token = mfaToken;
            dbUser.ClientUrl = clientUrl;

            var result = await _userManager.UpdateAsync(dbUser);
            if (!result.Succeeded)
            {
                var errorMessage = JsonSerializer.Serialize(result.Errors.Select(s => s.Description).ToArray());
                throw new BadRequestException(errorMessage);
            }

            return true;
        }

        public async Task<AuthResponseModel> VerifyTwoFactorTokenAsync(string userId, string provider, string token, CancellationToken cancellationToken)
        {
            var dbUser = await _userManager.Users.Include(i => i.UserRoles)
                                                 .ThenInclude(i => i.Role)
                                                 .Where(w => w.Id == userId)
                                                 .AsNoTracking()
                                                 .FirstOrDefaultAsync(cancellationToken);

            if (dbUser == null)
                throw new BadRequestException("Invalid token.");

            var result = await _userManager.VerifyTwoFactorTokenAsync(dbUser, provider, token);
            if (result)
            {
                await _signInManager.SignInAsync(dbUser, false);

                var response = new AuthResponseModel
                {
                    FirstName = dbUser.FirstName,
                    LastName = dbUser.LastName,
                    TokenType = AuthTokenType.JWT,
                    Token = GenerateToken(dbUser)
                };
                return response;
            }

            throw new BadRequestException("Invalid token.");
        }

        private async Task<AuthResponseModel> GetMfaTokenAsync(User dbUser, CancellationToken cancellationToken)
        {
            var token = await _userManager.GenerateTwoFactorTokenAsync(dbUser, $"{dbUser.DefaultMfaProvider}");
            dbUser.LastUpdatedAt = DateTimeOffset.UtcNow;
            dbUser.LastUpdatedBy = dbUser.Email;
            dbUser.Activity = dbUser.DefaultMfaProvider == MfaProvider.Email ? ActivityLog.MfaTokenToEmail : ActivityLog.MfaTokenToPhone;
            dbUser.Token = token;

            await _userManager.UpdateAsync(dbUser);

            var response = new AuthResponseModel
            {
                FirstName = dbUser.FirstName,
                LastName = dbUser.LastName,
                TokenType = AuthTokenType.MFA,
                Token = GenerateToken(dbUser, true, $"{dbUser.DefaultMfaProvider}")
            };
            return response;
        }

        private string GenerateToken(User user, bool isMfa = false, string mfaProvider = "")
        {
            try
            {
                // Initialize token handler
                var tokenHandler = new JwtSecurityTokenHandler();
                SymmetricSecurityKey securityKey;

                // Get secret key
                try
                {
                    var secretKey = Encoding.UTF8.GetBytes(_jwtConfig.Key);
                    securityKey = new SymmetricSecurityKey(secretKey);
                }
                catch (Exception ex)
                {
                    // Log or handle exception related to key generation
                    throw new Exception("Error while generating the security key.", ex);
                }

                // Prepare claims
                Claim[] claims;
                try
                {
                    claims = new Claim[]
                    {
                new Claim(JwtRegisteredClaimNames.Jti, $"{Guid.NewGuid()}"),
                new Claim(ClaimTypes.NameIdentifier, $"{user.Id}"),
                new Claim(ClaimTypes.Name, $"{user.FirstName} {user.LastName}"),
                new Claim(ClaimTypes.Email, $"{user.Email}")
                    };
                }
                catch (Exception ex)
                {
                    // Log or handle exception during claims creation
                    throw new Exception("Error while creating token claims.", ex);
                }

                // Add role-based claims
                var claimsIdentity = new ClaimsIdentity(claims);
                try
                {
                    claimsIdentity.AddClaims(user.UserRoles.Select(s => new Claim(ClaimTypes.Role, $"{s.Role.Name}")));
                }
                catch (Exception ex)
                {
                    // Log or handle exception when adding role claims
                    throw new Exception("Error while adding role claims.", ex);
                }

                // Add MFA claims if applicable
                if (isMfa)
                {
                    try
                    {
                        claimsIdentity.AddClaim(new Claim(ClaimTypes.AuthenticationMethod, "mfa"));
                        claimsIdentity.AddClaim(new Claim(ClaimTypes.AuthorizationDecision, mfaProvider));
                    }
                    catch (Exception ex)
                    {
                        // Log or handle exception when adding MFA claims
                        throw new Exception("Error while adding MFA claims.", ex);
                    }
                }

                // Token descriptor setup
                SecurityTokenDescriptor tokenDescriptor;
                try
                {
                    tokenDescriptor = new SecurityTokenDescriptor
                    {
                        Audience = _jwtConfig.Audience,
                        Subject = claimsIdentity,
                        Expires = DateTime.UtcNow.AddMinutes(_jwtConfig.ExpireInMinutes),
                        SigningCredentials = new SigningCredentials(securityKey, SecurityAlgorithms.HmacSha512),
                        Issuer = _jwtConfig.Issuer
                    };
                }
                catch (Exception ex)
                {
                    // Log or handle exception when creating token descriptor
                    throw new Exception("Error while creating token descriptor.", ex);
                }

                // Create the token
                SecurityToken token;
                try
                {
                    token = tokenHandler.CreateJwtSecurityToken(tokenDescriptor);
                }
                catch (Exception ex)
                {
                    // Log or handle exception during token creation
                    throw new Exception("Error while creating JWT token.", ex);
                }

                // Write token and return
                try
                {
                    return tokenHandler.WriteToken(token);
                }
                catch (Exception ex)
                {
                    // Log or handle exception while writing the token
                    throw new Exception("Error while writing the JWT token.", ex);
                }
            }
            catch (Exception ex)
            {
                // Log or rethrow any unhandled exceptions
                throw new Exception("An error occurred while generating the JWT token.", ex);
            }
        }


        #endregion

        #region MFA Setup
        public async Task<bool> RequestVerificationCodeAsync(string userId, string phoneNumber, string clientUrl, CancellationToken cancellationToken)
        {
            var dbUser = await _userManager.FindByIdAsync(userId);
            if (dbUser == null)
                throw new NotFoundException();

            if (await _userManager.Users.AnyAsync(a => a.PhoneNumber == phoneNumber && a.Id != userId, cancellationToken))
                throw new BadRequestException(string.Format(ExceptionMessage.PhonenumberTaken, phoneNumber));

            var token = await _userManager.GenerateChangePhoneNumberTokenAsync(dbUser, phoneNumber);

            dbUser.Activity = ActivityLog.ChangePhoneNumber;
            dbUser.PhoneNumber = phoneNumber;
            dbUser.Token = token;
            dbUser.ClientUrl = $"{clientUrl}/login";
            dbUser.LastUpdatedAt = DateTimeOffset.UtcNow;
            dbUser.LastUpdatedBy = dbUser.Email;

            var result = await _userManager.UpdateAsync(dbUser);
            if (!result.Succeeded)
            {
                var errorMessage = JsonSerializer.Serialize(result.Errors.Select(s => s.Description).ToArray());
                throw new BadRequestException(errorMessage);
            }
            return result.Succeeded;
        }

        public async Task<bool> VerifyPhoneNumberAsync(string userId, string token, string phoneNumber, string clientUrl, CancellationToken cancellationToken)
        {
            var dbUser = await _userManager.FindByIdAsync(userId);
            if (dbUser == null)
                throw new NotFoundException();

            dbUser.Activity = ActivityLog.VerifyPhoneNumber;
            dbUser.ClientUrl = clientUrl;
            dbUser.LastUpdatedAt = DateTimeOffset.UtcNow;
            dbUser.LastUpdatedBy = dbUser.Email;

            var result = await _userManager.ChangePhoneNumberAsync(dbUser, phoneNumber, token);
            if (!result.Succeeded)
            {
                var errorMessage = JsonSerializer.Serialize(result.Errors.Select(s => s.Description).ToArray());
                throw new BadRequestException(errorMessage);
            }
            return result.Succeeded;
        }

        public async Task<bool> RemovePhoneNumberAsync(string phoneNumber, string userId, string clientUrl, CancellationToken cancellationToken)
        {
            var dbUser = await _userManager.FindByIdAsync(userId);
            if (dbUser == null)
                throw new NotFoundException();
            if (dbUser.PhoneNumber != phoneNumber)
                throw new NotFoundException();

            dbUser.Activity = ActivityLog.RemovePhoneNumber;
            dbUser.ClientUrl = clientUrl;
            dbUser.PhoneNumberConfirmed = false;
            dbUser.LastUpdatedAt = DateTimeOffset.UtcNow;
            dbUser.LastUpdatedBy = dbUser.Email;
            // For notification
            dbUser.Token = phoneNumber;

            var result = await _userManager.SetPhoneNumberAsync(dbUser, "");
            if (!result.Succeeded)
            {
                var errorMessage = JsonSerializer.Serialize(result.Errors.Select(s => s.Description).ToArray());
                throw new BadRequestException(errorMessage);
            }
            return result.Succeeded;
        }

        public async Task<bool> EnableMfaAsync(string userId, MfaProvider defaultProvider, string clientUrl, CancellationToken cancellationToken)
        {
            var dbUser = await _userManager.FindByIdAsync(userId);
            if (dbUser == null)
                throw new NotFoundException();

            dbUser.Activity = ActivityLog.Enable2FA;
            dbUser.DefaultMfaProvider = defaultProvider;
            dbUser.ClientUrl = $"{clientUrl}/login";
            dbUser.LastUpdatedAt = DateTimeOffset.UtcNow;
            dbUser.LastUpdatedBy = dbUser.Email;

            var result = await _userManager.SetTwoFactorEnabledAsync(dbUser, true);
            if (!result.Succeeded)
            {
                var errorMessage = JsonSerializer.Serialize(result.Errors.Select(s => s.Description).ToArray());
                throw new BadRequestException(errorMessage);
            }
            return result.Succeeded;
        }

        public async Task<bool> DisableMfaAsync(string userId, string clientUrl, CancellationToken cancellationToken)
        {
            var dbUser = await _userManager.FindByIdAsync(userId);
            if (dbUser == null)
                throw new NotFoundException();

            dbUser.Activity = ActivityLog.Disable2FA;
            dbUser.ClientUrl = $"{clientUrl}/login";
            dbUser.LastUpdatedAt = DateTimeOffset.UtcNow;
            dbUser.LastUpdatedBy = dbUser.Email;

            var result = await _userManager.SetTwoFactorEnabledAsync(dbUser, false);
            if (!result.Succeeded)
            {
                var errorMessage = JsonSerializer.Serialize(result.Errors.Select(s => s.Description).ToArray());
                throw new BadRequestException(errorMessage);
            }
            return result.Succeeded;
        }
        #endregion

        public async Task<MfaProvider[]> ListUserMfaProvidersAsync(string userId, CancellationToken cancellationToken)
        {
            var dbUser = await _userManager.FindByIdAsync(userId);
            if (dbUser == null)
                throw new NotFoundException();

            var proviers = new List<MfaProvider>();
            if (dbUser.EmailConfirmed)
                proviers.Add(MfaProvider.Email);
            if (dbUser.PhoneNumberConfirmed)
                proviers.Add(MfaProvider.Phone);

            return proviers.ToArray();
        }

        public async Task<bool> ActivateAsync(string email, string token, string password)
        {
            var codeEncodedBytes = WebEncoders.Base64UrlDecode(token);
            var codeEncoded = Encoding.UTF8.GetString(codeEncodedBytes);

            var dbUser = await GetByEmailAsync(email);
            var result = await _userManager.ConfirmEmailAsync(dbUser, codeEncoded);
            if (!result.Succeeded)
            {
                var errorMessage = JsonSerializer.Serialize(result.Errors.Select(s => s.Description).ToArray());
                throw new BadRequestException(errorMessage);
            }

            // Always add password after the email has been confirmed.
            await _userManager.AddPasswordAsync(dbUser, password);

            return result.Succeeded;
        }

        public async Task<bool> ResetPasswordAsync(User dbUser, string token, string password)
        {
            var codeEncodedBytes = WebEncoders.Base64UrlDecode(token);
            var codeEncoded = Encoding.UTF8.GetString(codeEncodedBytes);

            var result = await _userManager.ResetPasswordAsync(dbUser, codeEncoded, password);
            if (!result.Succeeded)
            {
                var errorMessage = JsonSerializer.Serialize(result.Errors.Select(s => s.Description).ToArray());
                throw new BadRequestException(errorMessage);
            }
            return result.Succeeded;
        }

        public async Task<bool> CheckPasswordAsync(User dbUser, string password)
        {
            var result = await _userManager.CheckPasswordAsync(dbUser, password);
            if (!result)
            {
                throw new BadRequestException(ExceptionMessage.InvalidEmailPassword);
            }
            return result;
        }

        public async Task<bool> CreateAsync(User dbUser, string roleId)
        {
            var dbRole = await _roleManager.FindByIdAsync(roleId);
            if (dbRole == null)
                throw new BadRequestException(ExceptionMessage.InvalidRoleId);

            if (await _userManager.Users.AnyAsync(a => a.Email.ToLower() == dbUser.Email.ToLower()))
                throw new BadRequestException(string.Format(ExceptionMessage.EmailTaken, dbUser.Email));
            else if (await _userManager.Users.AnyAsync(a => a.UserName.ToLower() == dbUser.UserName.ToLower()))
                throw new BadRequestException(string.Format(ExceptionMessage.UsernameTaken, dbUser.UserName));
            else if (await _userManager.Users.AnyAsync(a => a.PhoneNumber == dbUser.PhoneNumber))
                throw new BadRequestException(string.Format(ExceptionMessage.PhonenumberTaken, dbUser.PhoneNumber));

            var result = await _userManager.CreateAsync(dbUser);
            if (result.Succeeded)
            {
                await _userManager.AddToRoleAsync(dbUser, dbRole.Name);
                return true;
            }

            var errorMessage = JsonSerializer.Serialize(result.Errors.Select(s => s.Description).ToArray());
            throw new BadRequestException(errorMessage);
        }

        public async Task<bool> DeleteAsync(User dbUser)
        {
            var result = await _userManager.DeleteAsync(dbUser);
            if (!result.Succeeded)
            {
                var errorMessage = JsonSerializer.Serialize(result.Errors.Select(s => s.Description).ToArray());
                throw new BadRequestException(errorMessage);
            }

            return result.Succeeded;
        }

        public async Task<bool> ReconfirmUserEmailAsync(User dbUser)
        {
            var hasPassword = await _userManager.HasPasswordAsync(dbUser);
            if (hasPassword)
            {
                await _userManager.RemovePasswordAsync(dbUser);
            }

            var result = await _userManager.UpdateAsync(dbUser);
            if (!result.Succeeded)
            {
                var errorMessage = JsonSerializer.Serialize(result.Errors.Select(s => s.Description).ToArray());
                throw new BadRequestException(errorMessage);
            }

            return result.Succeeded;
        }

        public async Task<string> GenerateEmailConfirmationTokenAsync(User dbUser)
        {
            var token = await _userManager.GenerateEmailConfirmationTokenAsync(dbUser);
            byte[] tokenGeneratedBytes = Encoding.UTF8.GetBytes(token);
            var codeEncoded = WebEncoders.Base64UrlEncode(tokenGeneratedBytes);

            return codeEncoded;
        }

        public async Task<string> GenerateResetPasswordTokenAsync(User dbUser)
        {
            var token = await _userManager.GeneratePasswordResetTokenAsync(dbUser);
            byte[] tokenGeneratedBytes = Encoding.UTF8.GetBytes(token);
            var codeEncoded = WebEncoders.Base64UrlEncode(tokenGeneratedBytes);

            return codeEncoded;
        }

        public async Task<User> GetByEmailAsync(string email)
        {
            var dbUser = await _userManager.Users.Include(i => i.UserRoles)
                                                 .ThenInclude(i => i.Role)
                                                 .FirstOrDefaultAsync(fd => fd.NormalizedEmail == email.ToUpper());
            if (dbUser == null)
                throw new NotFoundException(ExceptionMessage.InvalidUserRequest);

            return dbUser;
        }

        public async Task<User> GetByIdAsync(string id)
        {
            var dbUser = await _userManager.Users.Include(i => i.UserRoles)
                                                       .ThenInclude(i => i.Role)   
                                                 .FirstOrDefaultAsync(fd => fd.Id == id);
            if (dbUser == null)
            {
                throw new NotFoundException(ExceptionMessage.InvalidUserRequest);
            }
            return dbUser;
        }

        public async Task<User> GetByNameAsync(string username)
        {
            var dbUser = await _userManager.Users
                                                 .FirstOrDefaultAsync(fd => fd.NormalizedUserName == username.ToUpper());
            if (dbUser == null)
            {
                throw new NotFoundException(ExceptionMessage.InvalidEmailPassword);
            }
            return dbUser;
        }

        public async Task<Role> GetRoleByNameAsync(string roleName)
        {
            var dbRole = await _roleManager.FindByNameAsync(roleName);
            return dbRole;
        }

        public async Task<bool> UpdateAsync(User dbUser)
        {
            var result = await _userManager.UpdateAsync(dbUser);
            if (result.Succeeded)
            {
                return true;
            }

            var errorMessage = JsonSerializer.Serialize(result.Errors.Select(s => s.Description).ToArray());
            throw new BadRequestException(errorMessage);
        }

        public async Task<List<User>> ListUsersAsync(string roleName, CancellationToken cancellationToken)
        {
            var dbUsers = await (from u in _userManager.Users
                                 join ur in _userManager.Users.SelectMany(s => s.UserRoles.Select(s => new { s.UserId, s.RoleId }))
                                 on u.Id equals ur.UserId
                                 join r in _roleManager.Roles.Where(w => w.Name == roleName)
                                 on ur.RoleId equals r.Id
                                 select new User
                                 {
                                     Id = u.Id,
                                     FirstName = u.FirstName,
                                     LastName = u.LastName,
                                     Email = u.Email,
                                     PhoneNumber = u.PhoneNumber,
                                     UserName = u.UserName,
                                     IsActive = u.IsActive,
                                     EmailConfirmed = u.EmailConfirmed
                                 }).ToListAsync(cancellationToken);

            return dbUsers;
        }
    }
}
