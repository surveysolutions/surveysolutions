using System;
using System.Data;
using System.Web;
using System.Web.Security;
using Ninject;
using Questionnaire.Core.Web.Helpers;
using RavenQuestionnaire.Core;
using RavenQuestionnaire.Core.Views.User;

namespace Questionnaire.Core.Web.Security
{
    public class QuestionnaireMembershipProvider : MembershipProvider
    {
        public ICommandInvoker CommandInvoker
        {
            get { return KernelLocator.Kernel.Get<ICommandInvoker>(); }
        }
        public IViewRepository ViewRepository
        {
            get { return KernelLocator.Kernel.Get<IViewRepository>(); }
        }

        public override MembershipUser CreateUser(string username, string password, string email,
                                                  string passwordQuestion, string passwordAnswer, bool isApproved,
                                                  object providerUserKey, out MembershipCreateStatus status)
        {
            throw new NotImplementedException();
           /* try
            {
                CommandInvoker.Execute(new CreateNewUserCommand(username, email, password, UserRoles.User.ToString()));
                status = MembershipCreateStatus.Success;
                return
                    MembershipInstance(new UserView("id", username, password, email, DateTime.Now,
                                                    new[] {UserRoles.User}));
            }
            catch (Exception e)
            {
                status = MembershipCreateStatus.UserRejected;
                return null;
            }*/
        }

        public override bool ChangePasswordQuestionAndAnswer(string username, string password, string newPasswordQuestion, string newPasswordAnswer)
        {
            throw new NotImplementedException();
        }

        /// <summary>
        /// Returns the password of the specified user from the database. 
        /// The <see cref="QuestionnaireMembershipProvider" /> class does not support this method. 
        /// </summary>
        /// <param name="username">The user to retrieve the password for.</param>
        /// <param name="answer">The password answer for the user.</param>
        /// <returns>Always throws a <see cref="NotSupportedException" /> exception.</returns>
        public override string GetPassword(string username, string answer)
        {
            throw new NotSupportedException("The method or operation is not implemented.");
        }

        public override bool ChangePassword(string username, string oldPassword, string newPassword)
        {
            throw new NotImplementedException();
        }

        public override string ResetPassword(string username, string answer)
        {
            throw new NotImplementedException();
        }

        public override void UpdateUser(MembershipUser user)
        {
            throw new NotImplementedException();
        }

        public override bool ValidateUser(string username, string password)
        {
            var user = this.ViewRepository.Load<UserViewInputModel, UserView>(
                new UserViewInputModel(username.ToLower(), // bad hack due to key insensitivity of login
                    password));
            return user != null && !user.IsLocked;

        }

        public override bool UnlockUser(string userName)
        {
         //TODO   (new PersonRepository()).UpdatePersonLockStatus(userName, false);
            return false;
        }

        /// <summary>
        /// Gets information from the data source for a membership user.
        /// </summary>
        /// <param name="providerUserKey">The unique identifier for the membership user to get information for.</param>
        /// <param name="userIsOnline"><c>true</c> to update the last-activity date/time stamp for the user; <c>false</c> to return user information without updating the last-activity date/time stamp for the user.</param>
        /// <returns>A <see cref="MembershipUser" /> object populated with the specified user's information from the data source.</returns>
        public override MembershipUser GetUser(object providerUserKey, bool userIsOnline)
        {
            if (!(providerUserKey is byte[]))
                return null;
            MembershipUser retval =
                HttpContext.Current.Items[providerUserKey] as MembershipUser;
            if (retval == null)
            {
                var person = this.ViewRepository.Load<UserViewInputModel, UserView>(new UserViewInputModel(providerUserKey.ToString()));
                if (person == null)
                    return null;
                retval = MembershipInstance(person);
                HttpContext.Current.Items[providerUserKey] = retval;
            }
            return retval;

        }


        static readonly string PERSON_TO_REQUEST = "FEF8050CBFAC46edBDF6B73C5C14DF0B.";
        /// <summary>
        /// Gets information from the data source for a membership user.
        /// </summary>
        /// <param name="username">The name of the user to get information for.</param>
        /// <param name="userIsOnline"><c>true</c> to update the last-activity date/time stamp for the user; <c>false</c> to return user information without updating the last-activity date/time stamp for the user.</param>
        /// <returns>A <see cref="MembershipUser" /> object populated with the specified user's information from the data source.</returns>
        public override MembershipUser GetUser(string username, bool userIsOnline)
        {
            if (string.IsNullOrEmpty(username)) //!Performance Issue we does not support empty login name, hence no such user
                return null;
            string person_key = PERSON_TO_REQUEST + username;
            MembershipUser retval =
                HttpContext.Current.Items[person_key] as MembershipUser;
            if (retval == null)
            {
                var person =
                    this.ViewRepository.Load<UserViewInputModel, UserView>(new UserViewInputModel(username, null));
                if (person == null)
                    return null;
                retval = MembershipInstance(person);
                HttpContext.Current.Items[person_key] = retval;
            }
            return retval;

        }

        /// <summary>
        /// Create new <see cref="MembershipUser" />.
        /// </summary>
        /// <param name="person">A <see cref="DataRow" /> object.</param>
        /// <returns>A <see cref="MembershipUser" /> object populated with the information from the <see cref="DataRow" />.</returns>
        private MembershipUser MembershipInstance(UserView person)
        {
            byte[] bsid = new System.Text.UTF8Encoding().GetBytes(person.UserId);
            DateTime now = DateTime.Now;
            MembershipUser mu = new MembershipUser(
                this.GetType().Name,
                person.UserName, 
                bsid,
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

        public override string GetUserNameByEmail(string email)
        {
            throw new NotImplementedException();
        }

        public override bool DeleteUser(string username, bool deleteAllRelatedData)
        {
            throw new NotSupportedException("The method or operation is not implemented.");
        }

        public override MembershipUserCollection GetAllUsers(int pageIndex, int pageSize, out int totalRecords)
        {
            throw new NotImplementedException();
        }

        /// <summary>
        /// Gets the number of users currently accessing an application. Always returns <c>0</c>.
        /// </summary>
        /// <returns>Always returns <c>0</c></returns>
        public override int GetNumberOfUsersOnline()
        {
            return 0;
        }

        public override MembershipUserCollection FindUsersByName(string usernameToMatch, int pageIndex, int pageSize, out int totalRecords)
        {
            throw new NotImplementedException();
        }

        public override MembershipUserCollection FindUsersByEmail(string emailToMatch, int pageIndex, int pageSize, out int totalRecords)
        {
            throw new NotImplementedException();
        }

        /// <summary>
        /// Indicates whether the membership provider is configured to allow users to retrieve their passwords.
        /// </summary>
        /// <value>false</value>
        public override bool EnablePasswordRetrieval
        {
            get { return false; }
        }

        public override bool EnablePasswordReset
        {
            get { return true; }
        }

        public override bool RequiresQuestionAndAnswer
        {
            get { return false; }
        }

        public override string ApplicationName
        {
            get { return _applicationName; }
            set { _applicationName = value; }
        }
        private string _applicationName = "Questionnaire";

        public override int MaxInvalidPasswordAttempts
        {
            get { return 5; }
        }

        /// <summary>
        /// Gets the number of minutes in which a maximum number of invalid password or 
        /// password-answer attempts are allowed before the membership user is locked out.
        /// </summary>
        /// <value>10</value>
        public override int PasswordAttemptWindow
        {
            get { return 10; }
        }

        /// <summary>
        /// Gets a value indicating whether the membership provider is configured to require a 
        /// unique e-mail address for each user name.
        /// </summary>
        /// <value>false</value>
        public override bool RequiresUniqueEmail
        {
            get { return true; }
        }

        /// <summary>
        /// Gets a value indicating the format for storing passwords in the membership data store.
        /// </summary>
        /// <value><see cref="MembershipPasswordFormat.Hashed" /></value>
        public override MembershipPasswordFormat PasswordFormat
        {
            get { return MembershipPasswordFormat.Hashed; }
        }

        public override int MinRequiredPasswordLength
        {
            get { return 6; }
        }

        public override int MinRequiredNonAlphanumericCharacters
        {
            get { return 1; }
        }

        /// <summary>
        /// Gets the regular expression used to evaluate a password.
        /// </summary>
        public override string PasswordStrengthRegularExpression
        {
            get
            {
                return @"(?=^.{8,40}$)(?=.*\d)(?=.*[a-z])(?=.*[A-Z])(?=.*[!@#$%^&*()_+}{&quot;&quot;:;'?/>.<,]).*$";
            }
        }
    }
}
