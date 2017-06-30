using System.Collections.Specialized;
using WB.Core.GenericSubdomains.Portable.ServiceLocation;
using WB.UI.Shared.Web.Configuration;
using WB.UI.Shared.Web.Extensions;

namespace WB.UI.Shared.Web.MembershipProvider.Settings
{
    public class MembershipProviderSettings
    {
        private static MembershipProviderSettings instance;

        public static MembershipProviderSettings Instance
        {
            get { return instance ?? (instance = new MembershipProviderSettings()); }
        }

        private static NameValueCollection GetSection()
        {
            return ServiceLocator.Current.GetInstance<IConfigurationManager>().MembershipSettings;
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