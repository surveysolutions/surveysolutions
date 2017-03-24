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
        private const string expirationTimeElementName = "expirationTime";
        private const string slidingExpirationElementName = "slidingExpiration";
        private const string nameElementName = "name";
        private const string httpOnlyElementName = "httpOnly";

        [ConfigurationProperty(expirationTimeElementName)]
        public int ExpirationTime 
        {
            get { return (int)base[expirationTimeElementName]; }
            set { base[expirationTimeElementName] = value; }
        }

        [ConfigurationProperty(slidingExpirationElementName)]
        public bool SlidingExpiration
        {
            get { return (bool)base[slidingExpirationElementName]; }
            set { base[slidingExpirationElementName] = value; }
        }

        [ConfigurationProperty(nameElementName)]
        public string Name
        {
            get { return (string)base[nameElementName]; }
            set { base[nameElementName] = value; }
        }
        [ConfigurationProperty(httpOnlyElementName)]
        public bool HttpOnly
        {
            get { return (bool)base[httpOnlyElementName]; }
            set { base[httpOnlyElementName] = value; }
        }
    }
}
