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

        const string ENABLEPASSWORDRETRIEVAL = "enablePasswordRetrieval";
        const string ENABLEPASSWORDRESET = "enablePasswordReset";
        const string REQUIRESQUESTIONANDANSWER = "requiresQuestionAndAnswer";
        const string MINREQUIREDPASSWORDLENGTH = "minRequiredPasswordLength";
        const string MINREQUIREDNONALPHANUMERICCHARACTERS = "minRequiredNonalphanumericCharacters";
        const string MAXINVALIDPASSWORDATTEMPTS = "maxInvalidPasswordAttempts";
        const string PASSWORDATTEMPTWINDOW = "passwordAttemptWindow";
        const string PASSWORDSTRENGTHREGULAREXPRESSION = "passwordStrengthRegularExpression";

        public bool EnablePasswordRetrieval { get; private set; }
        public bool EnablePasswordReset { get; private set; }
        public bool RequiresQuestionAndAnswer { get; private set; }
        public int MinRequiredPasswordLength { get; private set; }
        public int MinRequiredNonalphanumericCharacters { get; private set; }
        public int MaxInvalidPasswordAttempts { get; private set; }
        public int PasswordAttemptWindow { get; private set; }
        public string PasswordStrengthRegularExpression { get; private set; }

        #region Constructors and Destructors

        /// <summary>
        /// Initializes a new instance of the <see cref="MembershipProviderSettings"/> class.
        /// </summary>
        /// <param name="settingsCollection">
        /// The settings Collection.
        /// </param>
        private MembershipProviderSettings(NameValueCollection settingsCollection)
            : base(settingsCollection)
        {
            this.EnablePasswordRetrieval = GetBoolean(ENABLEPASSWORDRETRIEVAL, false);
            this.EnablePasswordReset = GetBoolean(ENABLEPASSWORDRESET, true);
            this.RequiresQuestionAndAnswer = GetBoolean(REQUIRESQUESTIONANDANSWER, false);
            this.MinRequiredPasswordLength = GetInt(MINREQUIREDPASSWORDLENGTH, 6);
            this.MinRequiredNonalphanumericCharacters = GetInt(MINREQUIREDNONALPHANUMERICCHARACTERS, 0);
            this.MaxInvalidPasswordAttempts = GetInt(MAXINVALIDPASSWORDATTEMPTS, 5);
            this.PasswordAttemptWindow = GetInt(PASSWORDATTEMPTWINDOW, 10);
            this.PasswordStrengthRegularExpression = GetString(PASSWORDSTRENGTHREGULAREXPRESSION, string.Empty);
        }

        #endregion
    }
}