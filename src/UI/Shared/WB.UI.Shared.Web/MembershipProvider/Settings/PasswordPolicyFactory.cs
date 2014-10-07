using WB.UI.Shared.Web.MembershipProvider.Accounts;

namespace WB.UI.Shared.Web.MembershipProvider.Settings
{
    public static class PasswordPolicyFactory
    {

        public static IPasswordPolicy CreatePasswordPolicy()
        {
            return new PasswordPolicy
                       {
                           IsPasswordQuestionRequired = MembershipProviderSettings.Instance.RequiresQuestionAndAnswer,
                           IsPasswordResetEnabled = MembershipProviderSettings.Instance.EnablePasswordReset,
                           IsPasswordRetrievalEnabled = MembershipProviderSettings.Instance.EnablePasswordRetrieval,
                           MaxInvalidPasswordAttempts = MembershipProviderSettings.Instance.MaxInvalidPasswordAttempts,
                           MinRequiredNonAlphanumericCharacters = MembershipProviderSettings.Instance.MinRequiredNonalphanumericCharacters,
                           PasswordAttemptWindow = MembershipProviderSettings.Instance.PasswordAttemptWindow,
                           PasswordMinimumLength = MembershipProviderSettings.Instance.MinRequiredPasswordLength,
                           PasswordStrengthRegularExpression = MembershipProviderSettings.Instance.PasswordStrengthRegularExpression
                       };
        }
    }
}