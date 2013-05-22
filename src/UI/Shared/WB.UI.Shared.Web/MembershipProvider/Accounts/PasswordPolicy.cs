
namespace WB.UI.Shared.Web.MembershipProvider.Accounts
{
    /// <summary>
    /// Default policy object
    /// </summary>
    public class PasswordPolicy : IPasswordPolicy
    {
        #region Implementation of IPasswordPolicy

        /// <summary>
        /// Gets number of invalid password or password-answer attempts allowed before the membership user is locked out
        /// </summary>
        public int MaxInvalidPasswordAttempts { get; set; }

        /// <summary>
        /// Gets a value indicating whether the membership provider is configured to require the user to answer a password question for password reset and retrieval.
        /// </summary>
        public bool IsPasswordQuestionRequired { get; set; }

        /// <summary>
        /// Gets whether the membership provider is configured to allow users to reset their passwords
        /// </summary>
        public bool IsPasswordResetEnabled { get; set; }

        /// <summary>
        /// Gets whether the membership provider is configured to allow users to retrieve their passwords
        /// </summary>
        public bool IsPasswordRetrievalEnabled { get; set; }

        /// <summary>
        /// Gets the number of minutes in which a maximum number of invalid password or password-answer attempts are allowed before the membership user is locked out.
        /// </summary>
        public int PasswordAttemptWindow { get; set; }

        /// <summary>
        /// Get minimum length required for a password
        /// </summary>
        public int PasswordMinimumLength { get; set; }

        /// <summary>
        /// Gets minimum number of special characters that must be present in a valid password
        /// </summary>
        public int MinRequiredNonAlphanumericCharacters { get; set; }

        /// <summary>
        /// Gets the regular expression used to evaluate a password
        /// </summary>
        public string PasswordStrengthRegularExpression { get; set; }

        #endregion
    }
}