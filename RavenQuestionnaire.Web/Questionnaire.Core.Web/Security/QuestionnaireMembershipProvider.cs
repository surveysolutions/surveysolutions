using Main.Core.View;
using Main.Core.View.User;
using Microsoft.Practices.ServiceLocation;
using Ncqrs;
using Ncqrs.Commanding.ServiceModel;
using System;
using System.Web;
using System.Web.Security;

namespace Questionnaire.Core.Web.Security
{
    public class QuestionnaireMembershipProvider : MembershipProvider
    {
        private static readonly string PERSON_TO_REQUEST = "FEF8050CBFAC46edBDF6B73C5C14DF0B.";

        private string applicationName = "Questionnaire";

        private IViewFactory<UserViewInputModel, UserView> ViewFactory
        {
            get { return ServiceLocator.Current.GetInstance<IViewFactory<UserViewInputModel, UserView>>(); }
        }

        public override string ApplicationName
        {
            get
            {
                return this.applicationName;
            }

            set
            {
                this.applicationName = value;
            }
        }

        public ICommandService CommandInvoker
        {
            get { return NcqrsEnvironment.Get<ICommandService>(); }
        }

        public override bool EnablePasswordReset
        {
            get
            {
                return true;
            }
        }

        public override bool EnablePasswordRetrieval
        {
            get
            {
                return false;
            }
        }

        public override int MaxInvalidPasswordAttempts
        {
            get
            {
                return 5;
            }
        }

        public override int MinRequiredNonAlphanumericCharacters
        {
            get
            {
                return 1;
            }
        }

        public override int MinRequiredPasswordLength
        {
            get
            {
                return 6;
            }
        }

        public override int PasswordAttemptWindow
        {
            get
            {
                return 10;
            }
        }

        public override MembershipPasswordFormat PasswordFormat
        {
            get
            {
                return MembershipPasswordFormat.Hashed;
            }
        }

        public override string PasswordStrengthRegularExpression
        {
            get
            {
                return @"(?=^.{8,40}$)(?=.*\d)(?=.*[a-z])(?=.*[A-Z])(?=.*[!@#$%^&*()_+}{&quot;&quot;:;'?/>.<,]).*$";
            }
        }

        public override bool RequiresQuestionAndAnswer
        {
            get
            {
                return false;
            }
        }

        public override bool RequiresUniqueEmail
        {
            get
            {
                return true;
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
                UserView person =
                    this.ViewFactory.Load(
                        new UserViewInputModel((Guid)providerUserKey));
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
                UserView person =
                    this.ViewFactory.Load(new UserViewInputModel(username, null));
                if (person == null)
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
            UserView user =
                this.ViewFactory.Load(
                    new UserViewInputModel(
                        username.ToLower(), 
                        // bad hack due to key insensitivity of login
                        password));
            return user != null && !user.IsLocked;
        }

        private MembershipUser MembershipInstance(UserView person)
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
    }
}