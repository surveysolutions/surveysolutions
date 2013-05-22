namespace WB.UI.Shared.Web.MembershipProvider.Accounts.PasswordStrategies
{
    using System;
    using System.Linq;

    /// <summary>
    /// Extension methods for password policies.
    /// </summary>
    public static class PasswordPolicyExtensions
    {
        private static readonly Random _random = new Random();

        /// <summary>
        /// Determines whether the password is valid by going through all defined policies.
        /// </summary>
        /// <param name="passwordPolicy">The password policy.</param>
        /// <param name="password">The password.</param>
        /// <returns>
        ///   <c>true</c> if the password is valid; otherwise, <c>false</c>.
        /// </returns>
        public static bool IsPasswordValid(this IPasswordPolicy passwordPolicy, string password)
        {
            var alphaCount = password.Count(ch => !char.IsLetterOrDigit(ch));
            if (alphaCount < passwordPolicy.MinRequiredNonAlphanumericCharacters)
                return false;
            return password.Length >= passwordPolicy.PasswordMinimumLength;
        }

        /// <summary>
        /// Generate a new password
        /// </summary>
        /// <param name="policy">Policy that should be used when generating a new password.</param>
        /// <returns>A password which is not encrypted.</returns>
        /// <remarks>Uses characters which can't be mixed up along with <![CDATA["@!?&%/\"]]> if non alphas are required</remarks>
        public static string GeneratePassword(this IPasswordPolicy policy)
        {
            var length = _random.Next(policy.PasswordMinimumLength, policy.PasswordMinimumLength + 5);
            var password = "";

            var allowedCharacters = "abcdefghjkmnopqrstuvxtzABCDEFGHJKLMNPQRSTUVXYZ23456789";
            var alphas = "@!?&%/\\";
            if (policy.MinRequiredNonAlphanumericCharacters > 0)
                allowedCharacters += alphas;

            var nonAlphaLeft = policy.MinRequiredNonAlphanumericCharacters;
            for (var i = 0; i < length; i++)
            {
                var ch = allowedCharacters[_random.Next(0, allowedCharacters.Length)];
                if (alphas.IndexOf(ch) != -1)
                    nonAlphaLeft--;

                if (length - i <= nonAlphaLeft)
                    ch = alphas[_random.Next(0, alphas.Length)];

                password += ch;
            }

            return password;
        }
    }
}