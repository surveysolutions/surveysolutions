using System;
using System.Configuration;

namespace WB.UI.Shared.Web.Configuration
{
    public class HqSecuritySection : ConfigurationSection
    {
        private const string passwordPolicyElementName = "passwordPolicy";
        private const string cookieSettingsElementName = "cookieSettings";

        [ConfigurationProperty(passwordPolicyElementName)]
        public PasswordPolicyElement PasswordPolicy
        {
            get { return ((PasswordPolicyElement)(base[passwordPolicyElementName])); }
            set { base[passwordPolicyElementName] = value; }
        }

        [ConfigurationProperty(cookieSettingsElementName)]
        public CookieSettingsElement CookieSettings
        {
            get { return ((CookieSettingsElement)(base[cookieSettingsElementName])); }
            set { base[cookieSettingsElementName] = value; }
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

    public class CookieSettingsElement : ConfigurationElement
    {
        private const string experationTimeElementName = "experationTime";
        private const string slidingExpirationElementName = "slidingExpiration";

        [ConfigurationProperty(experationTimeElementName)]
        public int ExperationTime 
        {
            get { return (int)base[experationTimeElementName]; }
            set { base[experationTimeElementName] = value; }
        }

        [ConfigurationProperty(slidingExpirationElementName)]
        public bool SlidingExpiration
        {
            get { return (bool)base[slidingExpirationElementName]; }
            set { base[slidingExpirationElementName] = value; }
        }
    }
}
