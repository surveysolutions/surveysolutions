using WB.UI.Shared.Web;

namespace WB.UI.Designer
{
    using System.Collections.Specialized;
    using System.Web.Configuration;

    /// <summary>
    /// The membership provider settings.
    /// </summary>
    public class MembershipProviderSettings : WebConfigHelper
    {
        private static MembershipProviderSettings instance;

        public static MembershipProviderSettings Instance
        {
            get
            {
                if (instance == null)
                {
                    var membershipSection = GetSection<MembershipSection>("system.web/membership");
                    instance =
                        new MembershipProviderSettings(
                            membershipSection.Providers[membershipSection.DefaultProvider].Parameters);
                }

                return instance;
            }
        }

        private MembershipProviderSettings(NameValueCollection customSettingsSection)
            : base(customSettingsSection) { }

        public bool EnablePasswordRetrieval
        {
            get { return this.GetBoolean("enablePasswordRetrieval", false); }
        }

        public bool EnablePasswordReset
        {
            get { return this.GetBoolean("enablePasswordReset", true); }
        }

        public bool RequiresQuestionAndAnswer
        {
            get { return this.GetBoolean("requiresQuestionAndAnswer", false); }
        }

        public int MinRequiredPasswordLength
        {
            get { return this.GetInt("minRequiredPasswordLength", 6); }
        }

        public int MinRequiredNonalphanumericCharacters
        {
            get { return this.GetInt("minRequiredNonalphanumericCharacters", 0); }
        }

        public int MaxInvalidPasswordAttempts
        {
            get { return this.GetInt("maxInvalidPasswordAttempts", 5); }
        }

        public int PasswordAttemptWindow
        {
            get { return this.GetInt("passwordAttemptWindow", 10); }
        }

        public string PasswordStrengthRegularExpression
        {
            get { return this.GetString("passwordStrengthRegularExpression", string.Empty); }
        }

        public bool RequiresUniqueEmail
        {
            get { return this.GetBoolean("requiresUniqueEmail", true); }
        }
    }
}