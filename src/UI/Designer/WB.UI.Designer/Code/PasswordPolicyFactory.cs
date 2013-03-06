using Designer.Web.Providers.Membership;

namespace Designer.Web
{
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