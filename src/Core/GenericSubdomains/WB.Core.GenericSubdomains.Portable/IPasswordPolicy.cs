
namespace WB.Core.GenericSubdomains.Portable
{
    /// <summary>
    /// Policy which defines how passwords should be handled in the membership provider.
    /// </summary>
    public interface IPasswordPolicy
    {
        /// <summary>
        /// Get minimum length required for a password
        /// </summary>
        int PasswordMinimumLength { get; }

        /// <summary>
        /// Gets the regular expression used to evaluate a password
        /// </summary>
        string PasswordStrengthRegularExpression { get; }
    }
}
