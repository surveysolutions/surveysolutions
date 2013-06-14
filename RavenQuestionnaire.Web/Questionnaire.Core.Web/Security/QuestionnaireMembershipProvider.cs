namespace Questionnaire.Core.Web.Security
{
    using System;
    using System.Data;
    using System.Web;
    using System.Web.Security;

    using Main.Core.View;
    using Main.Core.View.User;

    using Microsoft.Practices.ServiceLocation;

    using Ncqrs;
    using Ncqrs.Commanding.ServiceModel;

    using Ninject;

    using Questionnaire.Core.Web.Helpers;

    public class QuestionnaireMembershipProvider : MembershipProvider
    {
        private static readonly string PERSON_TO_REQUEST = "FEF8050CBFAC46edBDF6B73C5C14DF0B.";

        private readonly IViewFactory<UserViewInputModel, UserView> viewFactory;

        public QuestionnaireMembershipProvider(IViewFactory<UserViewInputModel, UserView> viewFactory)
        {
            this.viewFactory = viewFactory;
        }

        private string applicationName = "Questionnaire";

        #region Public Properties

        /// <summary>
        /// Gets or sets the application name.
        /// </summary>
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

        /// <summary>
        /// Gets the command invoker.
        /// </summary>
        public ICommandService CommandInvoker
        {
            get
            {
                return NcqrsEnvironment.Get<ICommandService>(); /*KernelLocator.Kernel.Get<ICommandInvoker>()*/
                ;
            }
        }

        /// <summary>
        /// Gets a value indicating whether enable password reset.
        /// </summary>
        public override bool EnablePasswordReset
        {
            get
            {
                return true;
            }
        }

        /// <summary>
        /// Indicates whether the membership provider is configured to allow users to retrieve their passwords.
        /// </summary>
        /// <value>false</value>
        public override bool EnablePasswordRetrieval
        {
            get
            {
                return false;
            }
        }

        /// <summary>
        /// Gets the max invalid password attempts.
        /// </summary>
        public override int MaxInvalidPasswordAttempts
        {
            get
            {
                return 5;
            }
        }

        /// <summary>
        /// Gets the min required non alphanumeric characters.
        /// </summary>
        public override int MinRequiredNonAlphanumericCharacters
        {
            get
            {
                return 1;
            }
        }

        /// <summary>
        /// Gets the min required password length.
        /// </summary>
        public override int MinRequiredPasswordLength
        {
            get
            {
                return 6;
            }
        }

        /// <summary>
        /// Gets the number of minutes in which a maximum number of invalid password or 
        /// password-answer attempts are allowed before the membership user is locked out.
        /// </summary>
        /// <value>10</value>
        public override int PasswordAttemptWindow
        {
            get
            {
                return 10;
            }
        }

        /// <summary>
        /// Gets a value indicating the format for storing passwords in the membership data store.
        /// </summary>
        /// <value><see cref="MembershipPasswordFormat.Hashed" /></value>
        public override MembershipPasswordFormat PasswordFormat
        {
            get
            {
                return MembershipPasswordFormat.Hashed;
            }
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

        /// <summary>
        /// Gets a value indicating whether requires question and answer.
        /// </summary>
        public override bool RequiresQuestionAndAnswer
        {
            get
            {
                return false;
            }
        }

        /// <summary>
        /// Gets a value indicating whether the membership provider is configured to require a 
        /// unique e-mail address for each user name.
        /// </summary>
        /// <value>false</value>
        public override bool RequiresUniqueEmail
        {
            get
            {
                return true;
            }
        }

        #endregion

        #region Public Methods and Operators

        /// <summary>
        /// The change password.
        /// </summary>
        /// <param name="username">
        /// The username.
        /// </param>
        /// <param name="oldPassword">
        /// The old password.
        /// </param>
        /// <param name="newPassword">
        /// The new password.
        /// </param>
        /// <returns>
        /// The System.Boolean.
        /// </returns>
        /// <exception cref="NotImplementedException">
        /// </exception>
        public override bool ChangePassword(string username, string oldPassword, string newPassword)
        {
            throw new NotImplementedException();
        }

        /// <summary>
        /// The change password question and answer.
        /// </summary>
        /// <param name="username">
        /// The username.
        /// </param>
        /// <param name="password">
        /// The password.
        /// </param>
        /// <param name="newPasswordQuestion">
        /// The new password question.
        /// </param>
        /// <param name="newPasswordAnswer">
        /// The new password answer.
        /// </param>
        /// <returns>
        /// The System.Boolean.
        /// </returns>
        /// <exception cref="NotImplementedException">
        /// </exception>
        public override bool ChangePasswordQuestionAndAnswer(
            string username, string password, string newPasswordQuestion, string newPasswordAnswer)
        {
            throw new NotImplementedException();
        }

        /// <summary>
        /// The create user.
        /// </summary>
        /// <param name="username">
        /// The username.
        /// </param>
        /// <param name="password">
        /// The password.
        /// </param>
        /// <param name="email">
        /// The email.
        /// </param>
        /// <param name="passwordQuestion">
        /// The password question.
        /// </param>
        /// <param name="passwordAnswer">
        /// The password answer.
        /// </param>
        /// <param name="isApproved">
        /// The is approved.
        /// </param>
        /// <param name="providerUserKey">
        /// The provider user key.
        /// </param>
        /// <param name="status">
        /// The status.
        /// </param>
        /// <returns>
        /// The System.Web.Security.MembershipUser.
        /// </returns>
        /// <exception cref="NotImplementedException">
        /// </exception>
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

        /// <summary>
        /// The delete user.
        /// </summary>
        /// <param name="username">
        /// The username.
        /// </param>
        /// <param name="deleteAllRelatedData">
        /// The delete all related data.
        /// </param>
        /// <returns>
        /// The System.Boolean.
        /// </returns>
        /// <exception cref="NotSupportedException">
        /// </exception>
        public override bool DeleteUser(string username, bool deleteAllRelatedData)
        {
            throw new NotSupportedException("The method or operation is not implemented.");
        }

        /// <summary>
        /// The find users by email.
        /// </summary>
        /// <param name="emailToMatch">
        /// The email to match.
        /// </param>
        /// <param name="pageIndex">
        /// The page index.
        /// </param>
        /// <param name="pageSize">
        /// The page size.
        /// </param>
        /// <param name="totalRecords">
        /// The total records.
        /// </param>
        /// <returns>
        /// The System.Web.Security.MembershipUserCollection.
        /// </returns>
        /// <exception cref="NotImplementedException">
        /// </exception>
        public override MembershipUserCollection FindUsersByEmail(
            string emailToMatch, int pageIndex, int pageSize, out int totalRecords)
        {
            throw new NotImplementedException();
        }

        /// <summary>
        /// The find users by name.
        /// </summary>
        /// <param name="usernameToMatch">
        /// The username to match.
        /// </param>
        /// <param name="pageIndex">
        /// The page index.
        /// </param>
        /// <param name="pageSize">
        /// The page size.
        /// </param>
        /// <param name="totalRecords">
        /// The total records.
        /// </param>
        /// <returns>
        /// The System.Web.Security.MembershipUserCollection.
        /// </returns>
        /// <exception cref="NotImplementedException">
        /// </exception>
        public override MembershipUserCollection FindUsersByName(
            string usernameToMatch, int pageIndex, int pageSize, out int totalRecords)
        {
            throw new NotImplementedException();
        }

        /// <summary>
        /// The get all users.
        /// </summary>
        /// <param name="pageIndex">
        /// The page index.
        /// </param>
        /// <param name="pageSize">
        /// The page size.
        /// </param>
        /// <param name="totalRecords">
        /// The total records.
        /// </param>
        /// <returns>
        /// The System.Web.Security.MembershipUserCollection.
        /// </returns>
        /// <exception cref="NotImplementedException">
        /// </exception>
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

        /// <summary>
        /// Returns the password of the specified user from the database. 
        /// The <see cref="QuestionnaireMembershipProvider"/> class does not support this method. 
        /// </summary>
        /// <param name="username">
        /// The user to retrieve the password for.
        /// </param>
        /// <param name="answer">
        /// The password answer for the user.
        /// </param>
        /// <returns>
        /// Always throws a <see cref="NotSupportedException"/> exception.
        /// </returns>
        public override string GetPassword(string username, string answer)
        {
            throw new NotSupportedException("The method or operation is not implemented.");
        }

        /// <summary>
        /// Gets information from the data source for a membership user.
        /// </summary>
        /// <param name="providerUserKey">
        /// The unique identifier for the membership user to get information for.
        /// </param>
        /// <param name="userIsOnline">
        /// <c>true</c> to update the last-activity date/time stamp for the user; <c>false</c> to return user information without updating the last-activity date/time stamp for the user.
        /// </param>
        /// <returns>
        /// A <see cref="MembershipUser"/> object populated with the specified user's information from the data source.
        /// </returns>
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
                    this.viewFactory.Load(
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

        /// <summary>
        /// Gets information from the data source for a membership user.
        /// </summary>
        /// <param name="username">
        /// The name of the user to get information for.
        /// </param>
        /// <param name="userIsOnline">
        /// <c>true</c> to update the last-activity date/time stamp for the user; <c>false</c> to return user information without updating the last-activity date/time stamp for the user.
        /// </param>
        /// <returns>
        /// A <see cref="MembershipUser"/> object populated with the specified user's information from the data source.
        /// </returns>
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
                    this.viewFactory.Load(new UserViewInputModel(username, null));
                if (person == null)
                {
                    return null;
                }

                retval = this.MembershipInstance(person);
                HttpContext.Current.Items[person_key] = retval;
            }

            return retval;
        }

        /// <summary>
        /// The get user name by email.
        /// </summary>
        /// <param name="email">
        /// The email.
        /// </param>
        /// <returns>
        /// The System.String.
        /// </returns>
        /// <exception cref="NotImplementedException">
        /// </exception>
        public override string GetUserNameByEmail(string email)
        {
            throw new NotImplementedException();
        }

        /// <summary>
        /// The reset password.
        /// </summary>
        /// <param name="username">
        /// The username.
        /// </param>
        /// <param name="answer">
        /// The answer.
        /// </param>
        /// <returns>
        /// The System.String.
        /// </returns>
        /// <exception cref="NotImplementedException">
        /// </exception>
        public override string ResetPassword(string username, string answer)
        {
            throw new NotImplementedException();
        }

        /// <summary>
        /// The unlock user.
        /// </summary>
        /// <param name="userName">
        /// The user name.
        /// </param>
        /// <returns>
        /// The System.Boolean.
        /// </returns>
        public override bool UnlockUser(string userName)
        {
            // TODO   (new PersonRepository()).UpdatePersonLockStatus(userName, false);
            return false;
        }

        /// <summary>
        /// The update user.
        /// </summary>
        /// <param name="user">
        /// The user.
        /// </param>
        /// <exception cref="NotImplementedException">
        /// </exception>
        public override void UpdateUser(MembershipUser user)
        {
            throw new NotImplementedException();
        }

        /// <summary>
        /// The validate user.
        /// </summary>
        /// <param name="username">
        /// The username.
        /// </param>
        /// <param name="password">
        /// The password.
        /// </param>
        /// <returns>
        /// The System.Boolean.
        /// </returns>
        public override bool ValidateUser(string username, string password)
        {
            UserView user =
                this.viewFactory.Load(
                    new UserViewInputModel(
                        username.ToLower(), 
                        // bad hack due to key insensitivity of login
                        password));
            return user != null && !user.IsLocked;
        }

        #endregion

        #region Methods

        /// <summary>
        /// Create new <see cref="MembershipUser"/>.
        /// </summary>
        /// <param name="person">
        /// A <see cref="DataRow"/> object.
        /// </param>
        /// <returns>
        /// A <see cref="MembershipUser"/> object populated with the information from the <see cref="DataRow"/>.
        /// </returns>
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

        #endregion
    }
}