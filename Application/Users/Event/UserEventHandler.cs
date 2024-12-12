using Application.Interface;
using Common.Configurations;
using Common.Helpers;
using Common.Models;
using Common.Resources;
using Domain.Entities;
using Domain.Enumeration;
using Domain.Events;
using MediatR;
using Microsoft.Extensions.Hosting;

namespace Application.Users.Event
{
    public class UserEventHandler : INotificationHandler<CreatedEvent>, INotificationHandler<UpdatedEvent>
    {
        private readonly IIdentityService _identityService;
        private readonly IEmailService _emailService;
        private readonly IHostingEnvironment _env;

        public UserEventHandler(IIdentityService identityService,
                                IEmailService emailService,
                                IHostingEnvironment env)
        {
            _identityService = identityService;
            _emailService = emailService;
            _env = env;
        }

        public async Task Handle (CreatedEvent notification, CancellationToken cancellationToken)
        {
            var dbUser = notification.GetEntity<User>();
            if(dbUser != null && dbUser.Activity == ActivityLog.Created)
            {
                await SendActivationLinkAsync(dbUser, cancellationToken);
            }
        }

        public async Task Handle (UpdatedEvent notification, CancellationToken cancellationToken)
        {
            var dbUser = notification.GetEntity<User>();
            if(dbUser != null)
            {
                switch (dbUser.Activity)
                {
                    case ActivityLog.Activated:
                        await SendAccountActivatedNotificationAsync(dbUser, cancellationToken);
                        break;
                    case ActivityLog.RequestPasswordReset:
                        await SendResetPasswordLinkAsync(dbUser, cancellationToken);
                        break;
                    case ActivityLog.PasswordReset:
                        await SendPasswordResetNotificationAsync(dbUser, cancellationToken);
                        break;
                    case ActivityLog.RequestReinvite:
                        await SendActivationLinkAsync(dbUser, cancellationToken);
                        break;
                    case ActivityLog.MfaTokenToEmail:
                        await NotifyMfaTokenToEmailAsync(dbUser, cancellationToken);
                        break;
                    case ActivityLog.Created:
                    default:
                        break;
                }
            }
        }

        private string CompanyLogoPath()
        {
            var logoPath = Path.Combine(_env.ContentRootPath, "App Data", "Images", "logo.png");
            return logoPath;
        }

        private async Task SendActivationLinkAsync(User dbUser, CancellationToken cancellationToken)
        {
            var token = await _identityService.GenerateEmailConfirmationTokenAsync(dbUser);
            var confirmationLink = $"{dbUser.ClientUrl}/accounts/activate?firstname={dbUser.FirstName}&lastname={dbUser.LastName}&email={dbUser.Email}&token={token}";
            string emailTemplate = await FileHelper.ReadEmailTemplateAsync(EmailConfiguration.ACCOUNT_CONFIRMATION, cancellationToken);
            var companyLogo = CompanyLogoPath();

            emailTemplate = emailTemplate.Replace("{{company_logo}}", companyLogo)
                                       .Replace("{{user_name}}", $"{dbUser.FirstName} {dbUser.LastName}")
                                       .Replace("{{application_name}}", EmailContent.ApplicationName)
                                       .Replace("{{confirmation_link}}", confirmationLink)
                                       .Replace("{{support}}", EmailContent.SupportLink)
                                       .Replace("{{faq}}", EmailContent.FAQ)
                                       .Replace("{{privacy_policy}}", EmailContent.PrivacyPolicy)
                                       .Replace("{{terms_conditions}}", EmailContent.TermsLink)
                                       .Replace("{{company_name}}", EmailContent.CompanyName)
                                       .Replace("{{current_year}}", DateTime.UtcNow.Year.ToString());
            var email = new EmailModel
            {
                To = dbUser.Email,
                Body = emailTemplate,
                Subject = EmailContent.AccountActivation
            };

            await _emailService.SendAsync(email, cancellationToken);
        }

        private async Task SendResetPasswordLinkAsync(User dbUser, CancellationToken cancellationToken)
        {
            var token = await _identityService.GenerateResetPasswordTokenAsync(dbUser);
            var resetLink = $"{dbUser.ClientUrl}/accounts/reset-password?firstname={dbUser.FirstName}&lastname={dbUser.LastName}&email={dbUser.Email}&token={token}";
            var companyLogo = CompanyLogoPath();

            string emailTemplate = await FileHelper.ReadEmailTemplateAsync(EmailConfiguration.RESET_PASSWORD_REQUEST, cancellationToken);

            emailTemplate = emailTemplate.Replace("{{company_logo}}", companyLogo)
                                       .Replace("{{user_name}}", $"{dbUser.FirstName} {dbUser.LastName}")
                                       .Replace("{{application_name}}", EmailContent.ApplicationName)
                                       .Replace("{{password_reset_link}}", resetLink)
                                       .Replace("{{support_email}}", EmailContent.SupportEmail)
                                       .Replace("{{support}}", EmailContent.SupportLink)
                                       .Replace("{{faq}}", EmailContent.FAQ)
                                       .Replace("{{privacy_policy}}", EmailContent.PrivacyPolicy)
                                       .Replace("{{current_year}}", DateTime.UtcNow.Year.ToString())
                                       .Replace("{{company_name}}", EmailContent.CompanyName)
                                       .Replace("{{terms_conditions}}", EmailContent.TermsLink);

            var email = new EmailModel
            {
                To = dbUser.Email,
                Body = emailTemplate,
                Subject = EmailContent.PasswordResetRequest
            };

            await _emailService.SendAsync(email, cancellationToken);
        }

        private async Task SendPasswordResetNotificationAsync(User dbUser, CancellationToken cancellationToken)
        {
            var loginLink = $"{dbUser.ClientUrl}";
            var companyLogo = CompanyLogoPath();

            string emailTemplate = await FileHelper.ReadEmailTemplateAsync(EmailConfiguration.PASSWORD_RESET, cancellationToken);

            emailTemplate = emailTemplate.Replace("{{company_logo}}", companyLogo)
                                       .Replace("{{user_name}}", $"{dbUser.FirstName} {dbUser.LastName}")
                                       .Replace("{{application_name}}", EmailContent.ApplicationName)
                                       .Replace("{{confirmation_link}}", loginLink)
                                       .Replace("{{support}}", EmailContent.SupportLink)
                                       .Replace("{{faq}}", EmailContent.FAQ)
                                       .Replace("{{privacy_policy}}", EmailContent.PrivacyPolicy)
                                       .Replace("{{current_year}}", DateTime.UtcNow.Year.ToString())
                                       .Replace("{{company_name}}", EmailContent.CompanyName)
                                       .Replace("{{terms_conditions}}", EmailContent.TermsLink);

            var email = new EmailModel
            {
                To = dbUser.Email,
                Body = emailTemplate,
                Subject = EmailContent.PasswordResetComplete
            };

            await _emailService.SendAsync(email, cancellationToken);
        }

        private async Task SendAccountActivatedNotificationAsync(User dbUser, CancellationToken cancellationToken)
        {
            var loginLink = $"{dbUser.ClientUrl}";
            var companyLogo = CompanyLogoPath();

            string emailTemplate = await FileHelper.ReadEmailTemplateAsync(EmailConfiguration.ACCOUNT_ACTIVATED, cancellationToken);

            emailTemplate = emailTemplate.Replace("{{company_logo}}", companyLogo)
                                       .Replace("{{user_name}}", $"{dbUser.FirstName} {dbUser.LastName}")
                                       .Replace("{{application_name}}", EmailContent.ApplicationName)
                                       .Replace("{{login_link}}", loginLink)
                                       .Replace("{{support}}", EmailContent.SupportLink)
                                       .Replace("{{faq}}", EmailContent.FAQ)
                                       .Replace("{{privacy_policy}}", EmailContent.PrivacyPolicy)
                                       .Replace("{{current_year}}", DateTime.UtcNow.Year.ToString())
                                       .Replace("{{company_name}}", EmailContent.CompanyName)
                                       .Replace("{{terms_conditions}}", EmailContent.TermsLink);

            var email = new EmailModel
            {
                To = dbUser.Email,
                Body = emailTemplate,
                Subject = EmailContent.AccountConfirmation
            };

            await _emailService.SendAsync(email, cancellationToken);
        }

        private async Task NotifyMfaTokenToEmailAsync(User dbUser, CancellationToken cancellationToken)
        {
            var companyLogo = CompanyLogoPath();
            string emailTemplate = await FileHelper.ReadEmailTemplateAsync(EmailConfiguration.OTP_CODE, cancellationToken);

            emailTemplate = emailTemplate.Replace("{{company_logo}}", companyLogo)
                                       .Replace("{{user_name}}", $"{dbUser.FirstName} {dbUser.LastName}")
                                       .Replace("{{application_name}}", EmailContent.ApplicationName)
                                       .Replace("{{otp_code}}", string.Format(EmailContent.OTP, dbUser.Token))
                                       .Replace("{{otp_expiry_minutes}}", EmailContent.OTPExpiryTime)
                                       .Replace("{{support}}", EmailContent.SupportLink)
                                       .Replace("{{faq}}", EmailContent.FAQ)
                                       .Replace("{{privacy_policy}}", EmailContent.PrivacyPolicy)
                                       .Replace("{{current_year}}", DateTime.UtcNow.Year.ToString())
                                       .Replace("{{company_name}}", EmailContent.CompanyName)
                                       .Replace("{{terms_conditions}}", EmailContent.TermsLink);

            var email = new EmailModel
            {
                To = dbUser.Email,
                Body = emailTemplate,
                Subject = EmailContent.OtpCode
            };

            await _emailService.SendAsync(email, cancellationToken);
        }
    }
}
