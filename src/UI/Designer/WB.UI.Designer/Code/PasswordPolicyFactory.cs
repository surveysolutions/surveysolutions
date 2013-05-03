namespace WB.UI.Designer
{
    using WB.UI.Designer.Providers.Membership;

    public static class PasswordPolicyFactory
    {
        public static IPasswordPolicy CreatePasswordPolicy(bool isLockingAccountPolicyForced)
        {
            return new PasswordPolicy
            {
                IsPasswordQuestionRequired = false,
                IsPasswordResetEnabled = true,
                IsPasswordRetrievalEnabled = false,
                MaxInvalidPasswordAttempts = isLockingAccountPolicyForced ? 5 : int.MaxValue,
                MinRequiredNonAlphanumericCharacters = 0,
                PasswordAttemptWindow = 10,
                PasswordMinimumLength = 10,
                PasswordStrengthRegularExpression = null
            };
        }
    }
}