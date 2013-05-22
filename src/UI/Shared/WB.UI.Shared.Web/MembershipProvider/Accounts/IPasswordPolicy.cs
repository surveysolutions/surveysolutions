
namespace WB.UI.Shared.Web.MembershipProvider.Accounts
{
    /// <summary>
    /// Policy which defines how passwords should be handled in the membership provider.
    /// </summary>
    public interface IPasswordPolicy
    {
        /// <summary>
        /// Gets number of invalid password or password-answer attempts allowed before the membership user is locked out
        /// </summary>
        int MaxInvalidPasswordAttempts { get; }

        /// <summary>
        /// Gets a value indicating whether the membership provider is configured to require the user to answer a password question for password reset and retrieval.
        /// </summary>
        bool IsPasswordQuestionRequired { get; }

        /// <summary>
        /// Gets whether the membership provider is configured to allow users to reset their passwords
        /// </summary>
        bool IsPasswordResetEnabled { get; }

        /// <summary>
        /// Gets whether the membership provider is configured to allow users to retrieve their passwords
        /// </summary>
        bool IsPasswordRetrievalEnabled { get; }

        /// <summary>
        /// Gets the number of minutes in which a maximum number of invalid password or password-answer attempts are allowed before the membership user is locked out.
        /// </summary>
        int PasswordAttemptWindow { get; }

        /// <summary>
        /// Get minimum length required for a password
        /// </summary>
        int PasswordMinimumLength { get; }

        /// <summary>
        /// Gets minimum number of special characters that must be present in a valid password
        /// </summary>
        int MinRequiredNonAlphanumericCharacters { get; }

        /// <summary>
        /// Gets the regular expression used to evaluate a password
        /// </summary>
        string PasswordStrengthRegularExpression { get; }
    }
}