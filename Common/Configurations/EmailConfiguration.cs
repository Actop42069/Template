using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Common.Configurations
{
    public class EmailConfiguration
    {
        public const string SECTION_NAME = "SMTP";
        public string Host { get; set; }
        public string Sender { get; set; }

        public const string ACCOUNT_ACTIVATED = "AccountActivated.html";
        public const string ACCOUNT_CONFIRMATION = "AccountConfirmation.html";
        public const string RESET_PASSWORD_REQUEST = "PasswordResetRequest.html";
        public const string PASSWORD_RESET = "PasswordReset.html";
        public const string OTP_CODE = "OtpCode.html";
        public const string BILLING_TEMPLATE = "BillingTemplate.html";
    }
}
