namespace Domain.Enumeration
{
    public enum ActivityLog
    {
        Created = 1,
        Activated = 2,
        RequestReinvite = 3,
        RequestPasswordReset = 4,
        PasswordReset = 5,
        VerifyCustomer = 6,
        MfaTokenToPhone = 7,
        MfaTokenToEmail = 8,
        Enable2FA = 9,
        Disable2FA = 10,
        ChangePhoneNumber = 11,
        VerifyPhoneNumber = 12,
        RemovePhoneNumber = 13
    }
}
