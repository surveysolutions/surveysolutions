// --------------------------------------------------------------------------------------------------------------------
// <copyright file="PasswordPolicyFactory.cs" company="">
//   
// </copyright>
// --------------------------------------------------------------------------------------------------------------------

namespace WB.UI.Designer
{
    using WB.UI.Shared.Web.MembershipProvider.Accounts;

    /// <summary>
    /// The password policy factory.
    /// </summary>
    public static class PasswordPolicyFactory
    {
        #region Public Methods and Operators

        /// <summary>
        /// The create password policy.
        /// </summary>
        /// <returns>
        /// The <see cref="IPasswordPolicy"/>.
        /// </returns>
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

        #endregion
    }
}