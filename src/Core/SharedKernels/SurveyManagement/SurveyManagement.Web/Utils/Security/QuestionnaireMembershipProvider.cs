using System;
using System.Collections.Specialized;
using System.Web;
using System.Web.Security;
using Microsoft.Practices.ServiceLocation;
using Ncqrs;
using WB.Core.Infrastructure.CommandBus;
using WB.Core.Infrastructure.ReadSide;
using WB.Core.Infrastructure.Transactions;
using WB.Core.SharedKernels.SurveyManagement.Web.Models.User;

namespace WB.Core.SharedKernels.SurveyManagement.Web.Utils.Security
{
    public class QuestionnaireMembershipProvider : MembershipProvider
    {
        private static readonly string PERSON_TO_REQUEST = "FEF8050CBFAC46edBDF6B73C5C14DF0B.";

        private IUserWebViewFactory ViewFactory
        {
            get { return ServiceLocator.Current.GetInstance<IUserWebViewFactory>(); }
        }

        public override string ApplicationName
        {
            get { return this._ApplicationName; }
            set { this._ApplicationName = value; }
        }

        public ICommandService CommandInvoker
        {
            get { return ServiceLocator.Current.GetInstance<ICommandService>(); }
        }

        private ITransactionManagerProvider TransactionProvider
        {
            get { return ServiceLocator.Current.GetInstance<ITransactionManagerProvider>(); }
        }

        public override bool EnablePasswordReset
        {
            get
            {
                return _EnablePasswordReset;
            }
        }

        public override bool EnablePasswordRetrieval
        {
            get
            {
                return _EnablePasswordRetrieval;
            }
        }

        public override int MaxInvalidPasswordAttempts
        {
            get
            {
                return _MaxInvalidPasswordAttempts;
            }
        }

        public override int MinRequiredNonAlphanumericCharacters
        {
            get
            {
                return _MinRequiredNonalphanumericCharacters;
            }
        }

        public override int MinRequiredPasswordLength
        {
            get
            {
                return _MinRequiredPasswordLength;
            }
        }

        public override int PasswordAttemptWindow
        {
            get
            {
                return _PasswordAttemptWindow;
            }
        }

        public override MembershipPasswordFormat PasswordFormat
        {
            get
            {
                return _PasswordFormat;
            }
        }

        public override string PasswordStrengthRegularExpression
        {
            get
            {
                return _PasswordStrengthRegularExpression;
            }
        }

        public override bool RequiresQuestionAndAnswer
        {
            get
            {
                return _RequiresQuestionAndAnswer;
            }
        }

        public override bool RequiresUniqueEmail
        {
            get
            {
                return _RequiresUniqueEmail;
            }
        }

        public override bool ChangePassword(string username, string oldPassword, string newPassword)
        {
            throw new NotImplementedException();
        }

        public override bool ChangePasswordQuestionAndAnswer(
            string username, string password, string newPasswordQuestion, string newPasswordAnswer)
        {
            throw new NotImplementedException();
        }

        public override MembershipUser CreateUser(
            string username, 
            string password, 
            string email, 
            string passwordQuestion, 
            string passwordAnswer, 
            bool isApproved, 
            object providerUserKey, 
            out MembershipCreateStatus status)
        {
            throw new NotImplementedException();
        }

        public override bool DeleteUser(string username, bool deleteAllRelatedData)
        {
            throw new NotSupportedException("The method or operation is not implemented.");
        }

        public override MembershipUserCollection FindUsersByEmail(
            string emailToMatch, int pageIndex, int pageSize, out int totalRecords)
        {
            throw new NotImplementedException();
        }

        public override MembershipUserCollection FindUsersByName(
            string usernameToMatch, int pageIndex, int pageSize, out int totalRecords)
        {
            throw new NotImplementedException();
        }

        public override MembershipUserCollection GetAllUsers(int pageIndex, int pageSize, out int totalRecords)
        {
            throw new NotImplementedException();
        }

        public override int GetNumberOfUsersOnline()
        {
            return 0;
        }

        public override string GetPassword(string username, string answer)
        {
            throw new NotSupportedException("The method or operation is not implemented.");
        }

        public override MembershipUser GetUser(object providerUserKey, bool userIsOnline)
        {
            if (!(providerUserKey is byte[]))
            {
                return null;
            }

            var retval = HttpContext.Current.Items[providerUserKey] as MembershipUser;
            if (retval == null)
            {
                UserWebView person =
                    this.ViewFactory.Load(
                        new UserWebViewInputModel((Guid)providerUserKey));
                if (person == null)
                {
                    return null;
                }

                retval = this.MembershipInstance(person);
                HttpContext.Current.Items[providerUserKey] = retval;
            }

            return retval;
        }

        public override MembershipUser GetUser(string username, bool userIsOnline)
        {
            if (string.IsNullOrEmpty(username))
            {
                // !Performance Issue we does not support empty login name, hence no such user
                return null;
            }

            string person_key = PERSON_TO_REQUEST + username;
            var retval = HttpContext.Current.Items[person_key] as MembershipUser;
            if (retval == null)
            {
                UserWebView person =
                    this.ViewFactory.Load(new UserWebViewInputModel(username, null));
                if (person == null || person.IsArchived)
                {
                    return null;
                }

                retval = this.MembershipInstance(person);
                HttpContext.Current.Items[person_key] = retval;
            }

            return retval;
        }

        public override string GetUserNameByEmail(string email)
        {
            throw new NotImplementedException();
        }

        public override string ResetPassword(string username, string answer)
        {
            throw new NotImplementedException();
        }

        public override bool UnlockUser(string userName)
        {
            // TODO   (new PersonRepository()).UpdatePersonLockStatus(userName, false);
            return false;
        }

        public override void UpdateUser(MembershipUser user)
        {
            throw new NotImplementedException();
        }

        public override bool ValidateUser(string username, string password)
        {
            var transactionManager = this.TransactionProvider.GetTransactionManager();
            var shouldUseOwnTransaction = !transactionManager.IsQueryTransactionStarted;

            if (shouldUseOwnTransaction)
            {
                transactionManager.BeginQueryTransaction();
            }
            try
            {
                UserWebView user =
                    this.ViewFactory.Load(
                        new UserWebViewInputModel(
                            username.ToLower(), 
                            // bad hack due to key insensitivity of login
                            password));
                return user != null && !user.isLockedBySupervisor && !user.IsLockedByHQ;
            }
            finally
            {
                if (shouldUseOwnTransaction)
                {
                    transactionManager.RollbackQueryTransaction();
                }
            }
        }

        private MembershipUser MembershipInstance(UserWebView person)
        {
            // byte[] bsid = new System.Text.UTF8Encoding().GetBytes(person.PublicKey);
            DateTime now = DateTime.Now;
            var mu = new MembershipUser(
                this.GetType().Name, 
                person.UserName, 
                person.PublicKey, 
                null, 
                null, 
                null, 
                true, 
                false, 
                person.CreationDate, 
                now, 
                now, 
                now, 
                new DateTime(0x6da, 1, 1));

            return mu;
        }

        //
        // Properties from web.config, default all to False
        //
        private string _ApplicationName;
        private bool _EnablePasswordReset;
        private bool _EnablePasswordRetrieval;
        private bool _RequiresQuestionAndAnswer;
        private bool _RequiresUniqueEmail;
        private int _MaxInvalidPasswordAttempts;
        private int _PasswordAttemptWindow;
        private int _MinRequiredPasswordLength;
        private int _MinRequiredNonalphanumericCharacters;
        private string _PasswordStrengthRegularExpression = @"(?=^.{8,40}$)(?=.*\d)(?=.*[a-z])(?=.*[A-Z])(?=.*[!@#$%^&*()_+}{&quot;&quot;:;'?/>.<,]).*$";
        private MembershipPasswordFormat _PasswordFormat = MembershipPasswordFormat.Hashed;

        public override void Initialize(string name, NameValueCollection config)
        {
            if (config == null)
                throw new ArgumentNullException("config");

            if (name == null || name.Length == 0)
                name = "CustomMembershipProvider";

            if (String.IsNullOrEmpty(config["description"]))
            {
                config.Remove("description");
                config.Add("description", "Custom Membership Provider");
            }

            base.Initialize(name, config);

            _ApplicationName = GetConfigValue(config["applicationName"],
                          System.Web.Hosting.HostingEnvironment.ApplicationVirtualPath);
            _MaxInvalidPasswordAttempts = Convert.ToInt32(
                          GetConfigValue(config["maxInvalidPasswordAttempts"], "5"));
            _PasswordAttemptWindow = Convert.ToInt32(
                          GetConfigValue(config["passwordAttemptWindow"], "10"));
            _MinRequiredNonalphanumericCharacters = Convert.ToInt32(
                          GetConfigValue(config["minRequiredNonalphanumericCharacters"], "1"));
            _MinRequiredPasswordLength = Convert.ToInt32(
                          GetConfigValue(config["minRequiredPasswordLength"], "6"));
            _EnablePasswordReset = Convert.ToBoolean(
                          GetConfigValue(config["enablePasswordReset"], "true"));
            _RequiresUniqueEmail = Convert.ToBoolean(
                          GetConfigValue(config["requiresUniqueEmail"], "false"));
            _RequiresQuestionAndAnswer = Convert.ToBoolean(
                          GetConfigValue(config["requiresQuestionAndAnswer"], "false"));
            _EnablePasswordRetrieval = Convert.ToBoolean(
                          GetConfigValue(config["enablePasswordRetrieval"], "false"));
            _PasswordStrengthRegularExpression = GetConfigValue(config["passwordStrengthRegularExpression"], _PasswordStrengthRegularExpression);

        }

        //
        // A helper function to retrieve config values from the configuration file.
        //  

        private string GetConfigValue(string configValue, string defaultValue)
        {
            if (string.IsNullOrEmpty(configValue))
                return defaultValue;

            return configValue;
        }
    }
}