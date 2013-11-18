using System.Configuration;
using WB.UI.Shared.Web.Extensions;

namespace WB.UI.Designer
{
    using System.Collections.Specialized;
    using System.Web.Configuration;

    public class MembershipProviderSettings
    {
        private static MembershipProviderSettings instance;

        public static MembershipProviderSettings Instance
        {
            get { return instance ?? (instance = new MembershipProviderSettings()); }
        }

        private static NameValueCollection GetSection()
        {
            var membershipSection = (MembershipSection) ConfigurationManager.GetSection("system.web/membership");
            return membershipSection.Providers[membershipSection.DefaultProvider].Parameters;
        }

        public bool EnablePasswordRetrieval
        {
            get { return GetSection().GetBool("enablePasswordRetrieval", false); }
        }

        public bool EnablePasswordReset
        {
            get { return GetSection().GetBool("enablePasswordReset", true); }
        }

        public bool RequiresQuestionAndAnswer
        {
            get { return GetSection().GetBool("requiresQuestionAndAnswer", false); }
        }

        public int MinRequiredPasswordLength
        {
            get { return GetSection().GetInt("minRequiredPasswordLength", 6); }
        }

        public int MinRequiredNonalphanumericCharacters
        {
            get { return GetSection().GetInt("minRequiredNonalphanumericCharacters", 0); }
        }

        public int MaxInvalidPasswordAttempts
        {
            get { return GetSection().GetInt("maxInvalidPasswordAttempts", 5); }
        }

        public int PasswordAttemptWindow
        {
            get { return GetSection().GetInt("passwordAttemptWindow", 10); }
        }

        public string PasswordStrengthRegularExpression
        {
            get { return GetSection().GetString("passwordStrengthRegularExpression", string.Empty); }
        }

        public bool RequiresUniqueEmail
        {
            get { return GetSection().GetBool("requiresUniqueEmail", true); }
        }
    }
}