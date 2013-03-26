using WB.UI.Designer.Providers.Membership;

namespace WB.UI.Designer
{
    using WB.UI.Designer.Providers.Membership;

    public static class PasswordPolicyFactory
    {
        public static IPasswordPolicy CreatePasswordPolicy()
        {
            return new PasswordPolicy
            {
                IsPasswordQuestionRequired = false,
                IsPasswordResetEnabled = true,
                IsPasswordRetrievalEnabled = false,
                MaxInvalidPasswordAttempts = 5,
                MinRequiredNonAlphanumericCharacters = 0,
                PasswordAttemptWindow = 10,
                PasswordMinimumLength = 6,
                PasswordStrengthRegularExpression = null
            };
        }
    }
}