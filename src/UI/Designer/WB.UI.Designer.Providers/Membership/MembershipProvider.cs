// --------------------------------------------------------------------------------------------------------------------
// <copyright file="MembershipProvider.cs" company="">
//   
// </copyright>
// <summary>
//   A membership provider which uses different components to make it more SOLID.
// </summary>
// --------------------------------------------------------------------------------------------------------------------

namespace WB.UI.Designer.Providers.Membership
{
    using System;
    using System.Collections.Generic;
    using System.Configuration.Provider;
    using System.Security.Cryptography;
    using System.Web;
    using System.Web.Mvc;
    using System.Web.Security;

    using WebMatrix.WebData;

    /// <summary>
    ///     A membership provider which uses different components to make it more SOLID.
    /// </summary>
    /// <remarks>
    ///     You need to register the services listed under "See also" in your inversion of control container. This provider
    ///     uses <see cref="DependencyResolver" /> to find all dependencies.
    /// </remarks>
    /// <seealso cref="IAccountRepository" />
    /// <seealso cref="IPasswordPolicy" />
    /// <seealso cref="IPasswordStrategy" />
    public class MembershipProvider : ExtendedMembershipProvider
    {
        #region Constants

        /// <summary>
        /// The token size in bytes.
        /// </summary>
        private const int TokenSizeInBytes = 16;

        #endregion

        #region Fields

        /// <summary>
        /// The _password policy.
        /// </summary>
        private IPasswordPolicy passwordPolicy;

        /// <summary>
        /// The _password strategy.
        /// </summary>
        private IPasswordStrategy passwordStrategy;

        /// <summary>
        /// The _user service.
        /// </summary>
        private IAccountRepository userService;

        #endregion

        #region Public Properties

        /// <summary>
        ///     The name of the application using the custom membership provider.
        /// </summary>
        /// <returns>
        ///     The name of the application using the custom membership provider.
        /// </returns>
        public override string ApplicationName { get; set; }

        /// <summary>
        ///     Gets a brief, friendly description suitable for display in administrative tools or other user interfaces (UIs).
        /// </summary>
        /// <returns>A brief, friendly description suitable for display in administrative tools or other UIs.</returns>
        public override string Description
        {
            get
            {
                return "A more friendly membership provider.";
            }
        }

        /// <summary>
        ///     Indicates whether the membership provider is configured to allow users to reset their passwords.
        /// </summary>
        /// <returns>
        ///     true if the membership provider supports password reset; otherwise, false. The default is true.
        /// </returns>
        public override bool EnablePasswordReset
        {
            get
            {
                return this.PasswordPolicy.IsPasswordResetEnabled;
            }
        }

        /// <summary>
        ///     Indicates whether the membership provider is configured to allow users to retrieve their passwords.
        /// </summary>
        /// <returns>
        ///     true if the membership provider is configured to support password retrieval; otherwise, false. The default is false.
        /// </returns>
        public override bool EnablePasswordRetrieval
        {
            get
            {
                return this.PasswordStrategy.IsPasswordsDecryptable && this.PasswordPolicy.IsPasswordRetrievalEnabled;
            }
        }

        /// <summary>
        ///     Gets the number of invalid password or password-answer attempts allowed before the membership user is locked out.
        /// </summary>
        /// <returns>
        ///     The number of invalid password or password-answer attempts allowed before the membership user is locked out.
        /// </returns>
        public override int MaxInvalidPasswordAttempts
        {
            get
            {
                return this.PasswordPolicy.MaxInvalidPasswordAttempts;
            }
        }

        /// <summary>
        ///     Gets the minimum number of special characters that must be present in a valid password.
        /// </summary>
        /// <returns>
        ///     The minimum number of special characters that must be present in a valid password.
        /// </returns>
        public override int MinRequiredNonAlphanumericCharacters
        {
            get
            {
                return this.PasswordPolicy.MinRequiredNonAlphanumericCharacters;
            }
        }

        /// <summary>
        ///     Gets the minimum length required for a password.
        /// </summary>
        /// <returns>
        ///     The minimum length required for a password.
        /// </returns>
        public override int MinRequiredPasswordLength
        {
            get
            {
                return this.PasswordPolicy.PasswordMinimumLength;
            }
        }

        /// <summary>
        ///     Gets the number of minutes in which a maximum number of invalid password or password-answer attempts are allowed before the membership user is locked out.
        /// </summary>
        /// <returns>
        ///     The number of minutes in which a maximum number of invalid password or password-answer attempts are allowed before the membership user is locked out.
        /// </returns>
        public override int PasswordAttemptWindow
        {
            get
            {
                return this.PasswordPolicy.PasswordAttemptWindow;
            }
        }

        /// <summary>
        ///     Gets a value indicating the format for storing passwords in the membership data store.
        /// </summary>
        /// <returns>
        ///     One of the <see cref="T:System.Web.Security.MembershipPasswordFormat" /> values indicating the format for storing passwords in the data store.
        /// </returns>
        public override MembershipPasswordFormat PasswordFormat
        {
            get
            {
                return this.PasswordStrategy.PasswordFormat;
            }
        }

        /// <summary>
        ///     Gets the regular expression used to evaluate a password.
        /// </summary>
        /// <returns>
        ///     A regular expression used to evaluate a password.
        /// </returns>
        public override string PasswordStrengthRegularExpression
        {
            get
            {
                return this.PasswordPolicy.PasswordStrengthRegularExpression;
            }
        }

        /// <summary>
        ///     Gets a value indicating whether the membership provider is configured to require the user to answer a password question for password reset and retrieval.
        /// </summary>
        /// <returns>
        ///     true if a password answer is required for password reset and retrieval; otherwise, false. The default is true.
        /// </returns>
        public override bool RequiresQuestionAndAnswer
        {
            get
            {
                return this.PasswordPolicy.IsPasswordQuestionRequired;
            }
        }

        /// <summary>
        ///     Gets a value indicating whether the membership provider is configured to require a unique e-mail address for each user name.
        /// </summary>
        /// <returns>
        ///     true if the membership provider requires a unique e-mail address; otherwise, false. The default is true.
        /// </returns>
        public override bool RequiresUniqueEmail
        {
            get
            {
                return this.AccountRepository.IsUniqueEmailRequired;
            }
        }

        #endregion

        #region Properties

        /// <summary>
        ///     Gets the repository
        /// </summary>
        protected IAccountRepository AccountRepository
        {
            get
            {
                if (this.userService == null)
                {
                    this.userService = DependencyResolver.Current.GetService<IAccountRepository>();
                    if (this.userService == null)
                    {
                        throw new InvalidOperationException(
                            "You need to assign a locator to the ServiceLocator property and it should be able to lookup IAccountRepository.");
                    }
                }

                return this.userService;
            }
        }

        /// <summary>
        ///     Gets password policy
        /// </summary>
        protected IPasswordPolicy PasswordPolicy
        {
            get
            {
                if (this.passwordPolicy == null)
                {
                    this.passwordPolicy = DependencyResolver.Current.GetService<IPasswordPolicy>();
                    if (this.passwordPolicy == null)
                    {
                        throw new InvalidOperationException(
                            "You need to add an IPasswordPolicy implementation to your IoC container.");
                    }
                }

                return this.passwordPolicy;
            }
        }

        /// <summary>
        ///     Gets password strategy
        /// </summary>
        protected IPasswordStrategy PasswordStrategy
        {
            get
            {
                if (this.passwordStrategy == null)
                {
                    this.passwordStrategy = DependencyResolver.Current.GetService<IPasswordStrategy>();
                    if (this.passwordStrategy == null)
                    {
                        throw new InvalidOperationException(
                            "You need to assign a locator to the ServiceLocator property and it should be able to lookup IPasswordStrategy.");
                    }
                }

                return this.passwordStrategy;
            }
        }

        #endregion

        #region Public Methods and Operators

        /// <summary>
        /// Processes a request to update the password for a membership user.
        /// </summary>
        /// <returns>
        /// true if the password was updated successfully; otherwise, false.
        /// </returns>
        /// <param name="username">
        /// The user to update the password for. 
        /// </param>
        /// <param name="oldPassword">
        /// The current password for the specified user. 
        /// </param>
        /// <param name="newPassword">
        /// The new password for the specified user. 
        /// </param>
        public override bool ChangePassword(string username, string oldPassword, string newPassword)
        {
            IMembershipAccount account = this.AccountRepository.Get(username);
            AccountPasswordInfo pwInfo = account.CreatePasswordInfo();
            if (!this.PasswordStrategy.Compare(pwInfo, oldPassword))
            {
                return false;
            }

            this.ValidatePassword(username, newPassword);

            account.Password = newPassword;
            pwInfo = account.CreatePasswordInfo();
            account.Password = this.PasswordStrategy.Encrypt(pwInfo);
            this.AccountRepository.Update(account, MembershipEventType.ChangePassword);
            return true;
        }

        /// <summary>
        /// Processes a request to update the password question and answer for a membership user.
        /// </summary>
        /// <returns>
        /// true if the password question and answer are updated successfully; otherwise, false.
        /// </returns>
        /// <param name="username">
        /// The user to change the password question and answer for. 
        /// </param>
        /// <param name="password">
        /// The password for the specified user. 
        /// </param>
        /// <param name="newPasswordQuestion">
        /// The new password question for the specified user. 
        /// </param>
        /// <param name="newPasswordAnswer">
        /// The new password answer for the specified user. 
        /// </param>
        public override bool ChangePasswordQuestionAndAnswer(
            string username, string password, string newPasswordQuestion, string newPasswordAnswer)
        {
            IMembershipAccount account = this.AccountRepository.Get(username);
            if (account == null)
            {
                return false;
            }

            var info = new AccountPasswordInfo(username, account.Password) { PasswordSalt = account.PasswordSalt };
            if (this.PasswordStrategy.Compare(info, password))
            {
                return false;
            }

            account.PasswordQuestion = newPasswordAnswer;
            account.PasswordAnswer = newPasswordAnswer;
            this.AccountRepository.Update(account, MembershipEventType.ChangePasswordQuestionAndAnswer);
            return true;
        }

        /// <summary>
        /// The confirm account.
        /// </summary>
        /// <param name="accountConfirmationToken">
        /// The account confirmation token.
        /// </param>
        /// <returns>
        /// The <see cref="bool"/>.
        /// </returns>
        public override bool ConfirmAccount(string accountConfirmationToken)
        {
            return this.ConfirmAccount(null, accountConfirmationToken);
        }

        /// <summary>
        /// The confirm account.
        /// </summary>
        /// <param name="userName">
        /// The user name.
        /// </param>
        /// <param name="accountConfirmationToken">
        /// The account confirmation token.
        /// </param>
        /// <returns>
        /// The <see cref="bool"/>.
        /// </returns>
        public override bool ConfirmAccount(string userName, string accountConfirmationToken)
        {
            bool isConfirmed = false;

            if (userName == null)
            {
                userName = this.AccountRepository.GetUserNameByConfirmationToken(accountConfirmationToken);
            }

            IMembershipAccount user = this.AccountRepository.Get(userName);
            if (user != null)
            {
                isConfirmed = user.IsConfirmed = true;
                this.AccountRepository.Update(user, MembershipEventType.ConfirmAccount);
            }

            return isConfirmed;
        }

        /// <summary>
        /// The create account.
        /// </summary>
        /// <param name="userName">
        /// The user name.
        /// </param>
        /// <param name="password">
        /// The password.
        /// </param>
        /// <param name="requireConfirmationToken">
        /// The require confirmation token.
        /// </param>
        /// <returns>
        /// The <see cref="string"/>.
        /// </returns>
        public override string CreateAccount(string userName, string password, bool requireConfirmationToken)
        {
            return this.CreateUserAndAccount(userName, password, requireConfirmationToken, null);
        }

        /// <summary>
        /// Adds a new membership user to the data source.
        /// </summary>
        /// <returns>
        /// A <see cref="T:System.Web.Security.MembershipUser"/> object populated with the information for the newly created user.
        /// </returns>
        /// <param name="username">
        /// The user name for the new user. 
        /// </param>
        /// <param name="password">
        /// The password for the new user. 
        /// </param>
        /// <param name="email">
        /// The e-mail address for the new user.
        /// </param>
        /// <param name="passwordQuestion">
        /// The password question for the new user.
        /// </param>
        /// <param name="passwordAnswer">
        /// The password answer for the new user
        /// </param>
        /// <param name="isApproved">
        /// Whether or not the new user is approved to be validated.
        /// </param>
        /// <param name="providerUserKey">
        /// The unique identifier from the membership data source for the user.
        /// </param>
        /// <param name="status">
        /// A <see cref="T:System.Web.Security.MembershipCreateStatus"/> enumeration value indicating whether the user was created successfully.
        /// </param>
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
            IMembershipAccount account = this.InternalCreateAccount(
                username, password, email, providerUserKey, isApproved, out status);
            if (status == MembershipCreateStatus.Success)
            {
                return this.CloneUser(account);
            }

            return null;
        }

        /// <summary>
        /// The create user and account.
        /// </summary>
        /// <param name="userName">
        /// The user name.
        /// </param>
        /// <param name="password">
        /// The password.
        /// </param>
        /// <param name="requireConfirmation">
        /// The require confirmation.
        /// </param>
        /// <param name="values">
        /// The values.
        /// </param>
        /// <returns>
        /// The <see cref="string"/>.
        /// </returns>
        /// <exception cref="MembershipCreateUserException">
        /// </exception>
        public override string CreateUserAndAccount(
            string userName, string password, bool requireConfirmation, IDictionary<string, object> values)
        {
            string token = string.Empty;

            string email = values == null ? string.Empty : (string)values["Email"];

            MembershipCreateStatus status;
            IMembershipAccount account = this.InternalCreateAccount(
                userName, password, email, null, !requireConfirmation, out status);
            if (status == MembershipCreateStatus.Success)
            {
                token = account.ConfirmationToken;
            }
            else
            {
                throw new MembershipCreateUserException(status);
            }

            return token;
        }

        /// <summary>
        /// The delete account.
        /// </summary>
        /// <param name="userName">
        /// The user name.
        /// </param>
        /// <returns>
        /// The <see cref="bool"/>.
        /// </returns>
        public override bool DeleteAccount(string userName)
        {
            return this.DeleteUser(userName, true);
        }

        /// <summary>
        /// Removes a user from the membership data source.
        /// </summary>
        /// <returns>
        /// true if the user was successfully deleted; otherwise, false.
        /// </returns>
        /// <param name="username">
        /// The name of the user to delete.
        /// </param>
        /// <param name="deleteAllRelatedData">
        /// true to delete data related to the user from the database; false to leave data related to the user in the database.
        /// </param>
        public override bool DeleteUser(string username, bool deleteAllRelatedData)
        {
            return this.AccountRepository.Delete(username, deleteAllRelatedData);
        }

        /// <summary>
        /// Gets a collection of membership users where the e-mail address contains the specified e-mail address to match.
        /// </summary>
        /// <returns>
        /// A <see cref="T:System.Web.Security.MembershipUserCollection"/> collection that contains a page of
        ///     <paramref name="pageSize"/>
        ///     <see cref="T:System.Web.Security.MembershipUser"/> objects beginning at the page specified by
        ///     <paramref name="pageIndex"/>
        ///     .
        /// </returns>
        /// <param name="emailToMatch">
        /// The e-mail address to search for.
        /// </param>
        /// <param name="pageIndex">
        /// The index of the page of results to return. <paramref name="pageIndex"/> is zero-based.
        /// </param>
        /// <param name="pageSize">
        /// The size of the page of results to return.
        /// </param>
        /// <param name="totalRecords">
        /// The total number of matched users.
        /// </param>
        public override MembershipUserCollection FindUsersByEmail(
            string emailToMatch, int pageIndex, int pageSize, out int totalRecords)
        {
            IEnumerable<IMembershipAccount> users = this.AccountRepository.FindByEmail(
                emailToMatch, pageIndex, pageSize, out totalRecords);
            return this.CloneUsers(users);
        }

        /// <summary>
        /// Gets a collection of membership users where the user name contains the specified user name to match.
        /// </summary>
        /// <returns>
        /// A <see cref="T:System.Web.Security.MembershipUserCollection"/> collection that contains a page of
        ///     <paramref name="pageSize"/>
        ///     <see cref="T:System.Web.Security.MembershipUser"/> objects beginning at the page specified by
        ///     <paramref name="pageIndex"/>
        ///     .
        /// </returns>
        /// <param name="usernameToMatch">
        /// The user name to search for.
        /// </param>
        /// <param name="pageIndex">
        /// The index of the page of results to return. <paramref name="pageIndex"/> is zero-based.
        /// </param>
        /// <param name="pageSize">
        /// The size of the page of results to return.
        /// </param>
        /// <param name="totalRecords">
        /// The total number of matched users.
        /// </param>
        public override MembershipUserCollection FindUsersByName(
            string usernameToMatch, int pageIndex, int pageSize, out int totalRecords)
        {
            IEnumerable<IMembershipAccount> users = this.AccountRepository.FindByUserName(
                usernameToMatch, pageIndex, pageSize, out totalRecords);
            return this.CloneUsers(users);
        }

        /// <summary>
        /// The generate password reset token.
        /// </summary>
        /// <param name="userName">
        /// The user name.
        /// </param>
        /// <param name="tokenExpirationInMinutesFromNow">
        /// The token expiration in minutes from now.
        /// </param>
        /// <returns>
        /// The <see cref="string"/>.
        /// </returns>
        /// <exception cref="NotImplementedException">
        /// </exception>
        public override string GeneratePasswordResetToken(string userName, int tokenExpirationInMinutesFromNow)
        {
            string token = string.Empty;

            if (!this.PasswordPolicy.IsPasswordResetEnabled)
            {
                throw new NotSupportedException("Password reset is not supported.");
            }

            IMembershipAccount user = this.AccountRepository.Get(userName);
            if (user != null)
            {
                user.PasswordResetToken = token = GenerateToken();
                user.PasswordResetExpirationDate = DateTime.UtcNow.AddMinutes(tokenExpirationInMinutesFromNow);
                this.AccountRepository.Update(user, MembershipEventType.ChangePasswordResetToken);
            }

            return token;
        }

        /// <summary>
        /// The get accounts for user.
        /// </summary>
        /// <param name="userName">
        /// The user name.
        /// </param>
        /// <returns>
        /// The <see>
        ///         <cref>ICollection</cref>
        ///     </see>
        ///     .
        /// </returns>
        /// <exception cref="NotImplementedException">
        /// </exception>
        public override ICollection<OAuthAccountData> GetAccountsForUser(string userName)
        {
            throw new NotImplementedException();
        }

        /// <summary>
        /// Gets a collection of all the users in the data source in pages of data.
        /// </summary>
        /// <returns>
        /// A <see cref="T:System.Web.Security.MembershipUserCollection"/> collection that contains a page of
        ///     <paramref name="pageSize"/>
        ///     <see cref="T:System.Web.Security.MembershipUser"/> objects beginning at the page specified by
        ///     <paramref name="pageIndex"/>
        ///     .
        /// </returns>
        /// <param name="pageIndex">
        /// The index of the page of results to return. <paramref name="pageIndex"/> is zero-based.
        /// </param>
        /// <param name="pageSize">
        /// The size of the page of results to return.
        /// </param>
        /// <param name="totalRecords">
        /// The total number of matched users.
        /// </param>
        public override MembershipUserCollection GetAllUsers(int pageIndex, int pageSize, out int totalRecords)
        {
            IEnumerable<IMembershipAccount> users = this.AccountRepository.FindAll(
                pageIndex, pageSize, out totalRecords);
            return this.CloneUsers(users);
        }

        /// <summary>
        /// The get create date.
        /// </summary>
        /// <param name="userName">
        /// The user name.
        /// </param>
        /// <returns>
        /// The <see cref="DateTime"/>.
        /// </returns>
        public override DateTime GetCreateDate(string userName)
        {
            IMembershipAccount user = this.AccountRepository.Get(userName);
            if (user == null)
            {
                return DateTime.MinValue;
            }

            return user.CreatedAt;
        }

        /// <summary>
        /// The get last password failure date.
        /// </summary>
        /// <param name="userName">
        /// The user name.
        /// </param>
        /// <returns>
        /// The <see cref="DateTime"/>.
        /// </returns>
        public override DateTime GetLastPasswordFailureDate(string userName)
        {
            IMembershipAccount user = this.AccountRepository.Get(userName);
            if (user == null)
            {
                return DateTime.MinValue;
            }

            return user.FailedPasswordWindowStartedAt;
        }

        /// <summary>
        ///     Gets the number of users currently accessing the application.
        /// </summary>
        /// <returns>
        ///     The number of users currently accessing the application.
        /// </returns>
        public override int GetNumberOfUsersOnline()
        {
            return this.AccountRepository.GetNumberOfUsersOnline();
        }

        /// <summary>
        /// Gets the password for the specified user name from the data source.
        /// </summary>
        /// <returns>
        /// The password for the specified user name.
        /// </returns>
        /// <param name="username">
        /// The user to retrieve the password for. 
        /// </param>
        /// <param name="answer">
        /// The password answer for the user. 
        /// </param>
        public override string GetPassword(string username, string answer)
        {
            if (!this.PasswordPolicy.IsPasswordRetrievalEnabled || !this.PasswordStrategy.IsPasswordsDecryptable)
            {
                throw new ProviderException("Password retrieval is not supported");
            }

            IMembershipAccount account = this.AccountRepository.Get(username);
            if (!account.PasswordAnswer.Equals(answer, StringComparison.OrdinalIgnoreCase))
            {
                throw new MembershipPasswordException("Answer to Password question was incorrect.");
            }

            return this.PasswordStrategy.Decrypt(account.Password);
        }

        /// <summary>
        /// The get password changed date.
        /// </summary>
        /// <param name="userName">
        /// The user name.
        /// </param>
        /// <returns>
        /// The <see cref="DateTime"/>.
        /// </returns>
        public override DateTime GetPasswordChangedDate(string userName)
        {
            IMembershipAccount user = this.AccountRepository.Get(userName);
            if (user == null)
            {
                return DateTime.MinValue;
            }

            return user.LastPasswordChangeAt;
        }

        /// <summary>
        /// The get password failures since last success.
        /// </summary>
        /// <param name="userName">
        /// The user name.
        /// </param>
        /// <returns>
        /// The <see cref="int"/>.
        /// </returns>
        public override int GetPasswordFailuresSinceLastSuccess(string userName)
        {
            IMembershipAccount user = this.AccountRepository.Get(userName);
            if (user == null)
            {
                return 0;
            }

            return user.FailedPasswordWindowAttemptCount;
        }

        /// <summary>
        /// Gets user information from the data source based on the unique identifier for the membership user. Provides an option to update the last-activity date/time stamp for the user.
        /// </summary>
        /// <returns>
        /// A <see cref="T:System.Web.Security.MembershipUser"/> object populated with the specified user's information from the data source.
        /// </returns>
        /// <param name="providerUserKey">
        /// The unique identifier for the membership user to get information for.
        /// </param>
        /// <param name="userIsOnline">
        /// true to update the last-activity date/time stamp for the user; false to return user information without updating the last-activity date/time stamp for the user.
        /// </param>
        public override MembershipUser GetUser(object providerUserKey, bool userIsOnline)
        {
            IMembershipAccount user = this.AccountRepository.GetByProviderKey(providerUserKey);
            if (user == null)
            {
                return null;
            }

            this.UpdateOnlineState(userIsOnline, user);

            return this.CloneUser(user);
        }

        /// <summary>
        /// Gets information from the data source for a user. Provides an option to update the last-activity date/time stamp for the user.
        /// </summary>
        /// <returns>
        /// A <see cref="T:System.Web.Security.MembershipUser"/> object populated with the specified user's information from the data source.
        /// </returns>
        /// <param name="username">
        /// The name of the user to get information for.
        /// </param>
        /// <param name="userIsOnline">
        /// true to update the last-activity date/time stamp for the user;
        ///     false to return user information without updating the last-activity date/time stamp for the user.
        /// </param>
        public override MembershipUser GetUser(string username, bool userIsOnline)
        {
            IMembershipAccount user = this.AccountRepository.Get(username);
            if (user == null)
            {
                return null;
            }

            this.UpdateOnlineState(userIsOnline, user);

            return this.CloneUser(user);
        }

        /// <summary>
        /// The get user id from password reset token.
        /// </summary>
        /// <param name="token">
        /// The token.
        /// </param>
        /// <returns>
        /// The <see cref="int"/>.
        /// </returns>
        /// <exception cref="NotImplementedException">
        /// </exception>
        public override int GetUserIdFromPasswordResetToken(string token)
        {
            throw new NotImplementedException();
        }

        /// <summary>
        /// Gets the user name associated with the specified e-mail address.
        /// </summary>
        /// <returns>
        /// The user name associated with the specified e-mail address. If no match is found, return null.
        /// </returns>
        /// <param name="email">
        /// The e-mail address to search for. 
        /// </param>
        public override string GetUserNameByEmail(string email)
        {
            return this.AccountRepository.GetUserNameByEmail(email);
        }

        /// <summary>
        /// The is confirmed.
        /// </summary>
        /// <param name="userName">
        /// The user name.
        /// </param>
        /// <returns>
        /// The <see cref="bool"/>.
        /// </returns>
        public override bool IsConfirmed(string userName)
        {
            IMembershipAccount user = this.AccountRepository.Get(userName);
            if (user == null)
            {
                return false;
            }

            return user.IsConfirmed;
        }

        /// <summary>
        /// Resets a user's password to a new, automatically generated password.
        /// </summary>
        /// <returns>
        /// The new password for the specified user.
        /// </returns>
        /// <param name="username">
        /// The user to reset the password for. 
        /// </param>
        /// <param name="answer">
        /// The password answer for the specified user. 
        /// </param>
        public override string ResetPassword(string username, string answer)
        {
            if (!this.PasswordPolicy.IsPasswordResetEnabled)
            {
                throw new NotSupportedException("Password reset is not supported.");
            }

            IMembershipAccount user = this.AccountRepository.Get(username);
            if (this.PasswordPolicy.IsPasswordQuestionRequired && answer == null)
            {
                throw new MembershipPasswordException("Password answer is empty and question/answer is required.");
            }

            if (!user.PasswordAnswer.Equals(answer, StringComparison.OrdinalIgnoreCase))
            {
                return null;
            }

            string newPassword = this.PasswordStrategy.GeneratePassword(this.PasswordPolicy);

            this.ValidatePassword(username, newPassword);

            var info = new AccountPasswordInfo(username, newPassword);
            user.Password = this.PasswordStrategy.Encrypt(info);
            user.PasswordSalt = info.PasswordSalt;
            this.AccountRepository.Update(user, MembershipEventType.ResetPassword);
            return newPassword;
        }

        /// <summary>
        /// The reset password with token.
        /// </summary>
        /// <param name="token">
        /// The token.
        /// </param>
        /// <param name="newPassword">
        /// The new password.
        /// </param>
        /// <returns>
        /// The <see cref="bool"/>.
        /// </returns>
        /// <exception cref="NotImplementedException">
        /// </exception>
        public override bool ResetPasswordWithToken(string token, string newPassword)
        {
            if (!this.PasswordPolicy.IsPasswordResetEnabled)
            {
                throw new NotSupportedException("Password reset is not supported.");
            }

            bool isPasswordChanged = false;
            IMembershipAccount user = this.AccountRepository.GetUserByResetPasswordToken(token);
            if (user != null && user.PasswordResetExpirationDate > DateTime.UtcNow && user.PasswordResetToken == token)
            {
                this.ValidatePassword(user.UserName, newPassword);

                var info = new AccountPasswordInfo(user.UserName, newPassword);
                user.Password = this.PasswordStrategy.Encrypt(info);
                user.PasswordSalt = info.PasswordSalt;
                this.AccountRepository.Update(user, MembershipEventType.ResetPassword);

                isPasswordChanged = true;
            }

            return isPasswordChanged;
        }

        /// <summary>
        /// Clears a lock so that the membership user can be validated.
        /// </summary>
        /// <returns>
        /// true if the membership user was successfully unlocked; otherwise, false.
        /// </returns>
        /// <param name="userName">
        /// The membership user whose lock status you want to clear.
        /// </param>
        public override bool UnlockUser(string userName)
        {
            IMembershipAccount user = this.AccountRepository.Get(userName);
            if (user == null)
            {
                return false;
            }

            user.IsLockedOut = false;
            user.FailedPasswordAnswerWindowAttemptCount = 0;
            user.FailedPasswordAnswerWindowStartedAt = DateTime.MinValue;
            user.FailedPasswordWindowAttemptCount = 0;
            user.FailedPasswordWindowStartedAt = DateTime.MinValue;
            this.AccountRepository.Update(user, MembershipEventType.UnlockUser);
            return true;
        }

        /// <summary>
        /// Updates information about a user in the data source.
        /// </summary>
        /// <param name="user">
        /// A <see cref="T:System.Web.Security.MembershipUser"/> object that represents the user to update and the updated information for the user.
        /// </param>
        public override void UpdateUser(MembershipUser user)
        {
            IMembershipAccount account = this.AccountRepository.Get(user.UserName);
            this.Merge(user, account);
            this.AccountRepository.Update(account);
        }

        /// <summary>
        /// Verifies that the specified user name and password exist in the data source.
        /// </summary>
        /// <returns>
        /// true if the specified username and password are valid; otherwise, false.
        /// </returns>
        /// <param name="username">
        /// The name of the user to validate. 
        /// </param>
        /// <param name="password">
        /// The password for the specified user. 
        /// </param>
        public override bool ValidateUser(string username, string password)
        {
            IMembershipAccount account = this.AccountRepository.Get(username);
            if (account == null)
            {
                return false;
            }

            AccountPasswordInfo passwordInfo = account.CreatePasswordInfo();
            bool validated = this.PasswordStrategy.Compare(passwordInfo, password);
            if (validated)
            {
                account.LastLoginAt = DateTime.Now;
                account.FailedPasswordWindowStartedAt = DateTime.MinValue;
                account.FailedPasswordWindowAttemptCount = 0;
                this.AccountRepository.Update(account, MembershipEventType.UserValidated);
                return true;
            }
            else
            {
                if (account.FailedPasswordWindowAttemptCount > this.PasswordPolicy.MaxInvalidPasswordAttempts)
                {
                    this.LockUser(account);
                }
                else
                {
                    account.FailedPasswordWindowAttemptCount += 1;
                    if (account.FailedPasswordWindowStartedAt == DateTime.MinValue)
                    {
                        account.FailedPasswordAnswerWindowStartedAt = DateTime.Now;
                    }

                    this.AccountRepository.Update(account, MembershipEventType.FailedLogin);

                    // if (DateTime.Now.Subtract(user.FailedPasswordAnswerWindowStartedAt).TotalMinutes >
                    // PasswordPolicy.PasswordAttemptWindow)
                    // {
                    // user.IsLockedOut = true;
                    // user.LastLockedOutAt = DateTime.Now;
                    // AccountRepository.Update(user, MembershipEventType.LockUser);
                    // }
                }
            }

            return false;
        }

        #endregion

        #region Methods

        /// <summary>
        /// The generate token.
        /// </summary>
        /// <param name="generator">
        /// The generator.
        /// </param>
        /// <returns>
        /// The <see cref="string"/>.
        /// </returns>
        internal static string GenerateToken(RandomNumberGenerator generator)
        {
            var tokenBytes = new byte[TokenSizeInBytes];
            generator.GetBytes(tokenBytes);
            return HttpServerUtility.UrlTokenEncode(tokenBytes);
        }

        /// <summary>
        /// Create a membershipuser from an membership account.
        /// </summary>
        /// <param name="account">
        /// The account.
        /// </param>
        /// <returns>
        /// Created user
        /// </returns>
        protected MembershipUser CloneUser(IMembershipAccount account)
        {
            return new MembershipUser(
                this.Name, 
                account.UserName, 
                account.ProviderUserKey, 
                account.Email, 
                account.PasswordQuestion, 
                account.Comment, 
                account.IsConfirmed, 
                account.IsLockedOut, 
                account.CreatedAt, 
                account.LastLoginAt, 
                account.LastActivityAt, 
                account.LastPasswordChangeAt, 
                account.LastLockedOutAt);
        }

        /// <summary>
        /// The generate token.
        /// </summary>
        /// <returns>
        /// The <see cref="string"/>.
        /// </returns>
        private static string GenerateToken()
        {
            using (var prng = new RNGCryptoServiceProvider())
            {
                return GenerateToken(prng);
            }
        }

        /// <summary>
        /// The clone users.
        /// </summary>
        /// <param name="users">
        /// The users.
        /// </param>
        /// <returns>
        /// The <see cref="MembershipUserCollection"/>.
        /// </returns>
        private MembershipUserCollection CloneUsers(IEnumerable<IMembershipAccount> users)
        {
            var members = new MembershipUserCollection();
            foreach (IMembershipAccount user in users)
            {
                members.Add(this.CloneUser(user));
            }

            return members;
        }

        /// <summary>
        /// The internal create account.
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
        /// <param name="providerUserKey">
        /// The provider user key.
        /// </param>
        /// <param name="isApproved">
        /// The is approved.
        /// </param>
        /// <param name="status">
        /// The status.
        /// </param>
        /// <returns>
        /// The <see cref="IMembershipAccount"/>.
        /// </returns>
        private IMembershipAccount InternalCreateAccount(
            string username, 
            string password, 
            string email, 
            object providerUserKey, 
            bool isApproved, 
            out MembershipCreateStatus status)
        {
            if (this.AccountRepository.IsUniqueEmailRequired && this.AccountRepository.GetUserNameByEmail(email) != null)
            {
                status = MembershipCreateStatus.DuplicateEmail;
                return null;
            }

            if (this.AccountRepository.Get(username) != null)
            {
                status = MembershipCreateStatus.DuplicateUserName;
                return null;
            }

            try
            {
                this.ValidatePassword(username, password);
            }
            catch
            {
                // not the smoothest approach, but the best 
                // considering the inconsistent password failure handling.
                status = MembershipCreateStatus.InvalidPassword;
                return null;
            }

            IMembershipAccount account = this.AccountRepository.Create(
                providerUserKey, this.ApplicationName, username, email);
            var passwordInfo = new AccountPasswordInfo(username, password);
            account.Password = this.PasswordStrategy.Encrypt(passwordInfo);
            account.PasswordSalt = passwordInfo.PasswordSalt;
            account.IsConfirmed = isApproved;
            if (!isApproved)
            {
                account.ConfirmationToken = GenerateToken();
            }

            status = this.AccountRepository.Register(account);

            return account;
        }

        /// <summary>
        /// The lock user.
        /// </summary>
        /// <param name="account">
        /// The account.
        /// </param>
        private void LockUser(IMembershipAccount account)
        {
            account.IsLockedOut = true;
            account.LastLockedOutAt = DateTime.Now;
            this.AccountRepository.Update(account, MembershipEventType.LockUser);
        }

        /// <summary>
        /// The merge.
        /// </summary>
        /// <param name="user">
        /// The user.
        /// </param>
        /// <param name="account">
        /// The account.
        /// </param>
        private void Merge(MembershipUser user, IMembershipAccount account)
        {
            account.Comment = user.Comment;
            account.IsConfirmed = user.IsApproved;
            account.Email = user.Email;
            account.PasswordQuestion = user.PasswordQuestion;
            account.IsLockedOut = user.IsLockedOut;

            // account.IsOnline = user.IsOnline;
            account.LastActivityAt = user.LastActivityDate;
            account.LastLockedOutAt = user.LastLockoutDate;
            account.LastPasswordChangeAt = user.LastPasswordChangedDate;
            account.ProviderUserKey = user.ProviderUserKey;
            account.UserName = user.UserName;
        }

        /// <summary>
        /// The update online state.
        /// </summary>
        /// <param name="userIsOnline">
        /// The user is online.
        /// </param>
        /// <param name="user">
        /// The user.
        /// </param>
        private void UpdateOnlineState(bool userIsOnline, IMembershipAccount user)
        {
            if (!userIsOnline)
            {
                return;
            }

            user.LastActivityAt = DateTime.Now;

            // user.IsOnline = true;
            this.AccountRepository.Update(user, MembershipEventType.UpdateOnlineState);
        }

        /// <summary>
        /// The validate password.
        /// </summary>
        /// <param name="username">
        /// The username.
        /// </param>
        /// <param name="clearTextPassword">
        /// The clear text password.
        /// </param>
        /// <exception cref="MembershipPasswordException">
        /// </exception>
        /// <exception cref="Exception">
        /// </exception>
        private void ValidatePassword(string username, string clearTextPassword)
        {
            if (!this.PasswordStrategy.IsValid(clearTextPassword, this.PasswordPolicy))
            {
                throw new MembershipPasswordException("Password failed validation");
            }

            var args = new ValidatePasswordEventArgs(username, clearTextPassword, false);
            this.OnValidatingPassword(args);
            if (args.FailureInformation != null)
            {
                throw args.FailureInformation;
            }
        }

        #endregion
    }
}