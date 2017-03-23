using System.Configuration;

namespace WB.UI.Shared.Web.Configuration
{
    public class HqSecuritySection : ConfigurationSection
    {
        private const string passwordPolicyElementName = "passwordPolicy";

        [ConfigurationProperty(passwordPolicyElementName)]
        public PasswordPolicyElement PasswordPolicy
        {
            get { return ((PasswordPolicyElement)(base[passwordPolicyElementName])); }
            set { base[passwordPolicyElementName] = value; }
        }
    }

    public class PasswordPolicyElement : ConfigurationElement
    {
        private const string minRequiredNonAlphanumericCharactersElementName = "minRequiredNonAlphanumericCharacters";
        private const string passwordMinimumLengthElementName = "passwordMinimumLength";
        private const string passwordStrengthRegularExpressionElementName = "passwordStrengthRegularExpression";

        [ConfigurationProperty(minRequiredNonAlphanumericCharactersElementName)]
        public int MinRequiredNonAlphanumericCharacters
        {
            get { return (int)base[minRequiredNonAlphanumericCharactersElementName]; }
            set { base[minRequiredNonAlphanumericCharactersElementName] = value; }
        }

        [ConfigurationProperty(passwordMinimumLengthElementName)]
        public int PasswordMinimumLength
        {
            get { return (int)base[passwordMinimumLengthElementName]; }
            set { base[passwordMinimumLengthElementName] = value; }
        }

        [ConfigurationProperty(passwordStrengthRegularExpressionElementName)]
        public string PasswordStrengthRegularExpression
        {
            get { return (string)base[passwordStrengthRegularExpressionElementName]; }
            set { base[passwordStrengthRegularExpressionElementName] = value; }
        }
    }
}
