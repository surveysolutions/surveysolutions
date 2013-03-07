using System;
using System.Collections.Generic;
using System.Configuration.Provider;
using System.Web.Security;
using WebMatrix.WebData;
using System.Web.Mvc;
using System.Security.Cryptography;
using System.Web;

namespace WB.UI.Designer.Providers.Membership
{
    /// <summary>
    /// A membership provider which uses different components to make it more SOLID.
    /// </summary>
    /// <remarks>
    /// You need to register the services listed under "See also" in your inversion of control container. This provider
    /// uses <see cref="DependencyResolver"/> to find all dependencies.
    /// </remarks>
    /// <seealso cref="IAccountRepository"/>
    /// <seealso cref="IPasswordPolicy"/>
    /// <seealso cref="IPasswordStrategy"/>
    public class MembershipProvider : ExtendedMembershipProvider
    {
        private const int TokenSizeInBytes = 16;

        private IPasswordPolicy _passwordPolicy;
        private IPasswordStrategy _passwordStrategy;
        private IAccountRepository _userService;

        /// <summary>
        /// Gets the repository
        /// </summary>
        protected IAccountRepository AccountRepository
        {
            get
            {
                if (_userService == null)
                {
                    _userService = DependencyResolver.Current.GetService<IAccountRepository>();
                    if (_userService == null)
                        throw new InvalidOperationException(
                            "You need to assign a locator to the ServiceLocator property and it should be able to lookup IAccountRepository.");
                }
                return _userService;
            }
        }

        /// <summary>
        /// Gets password strategy
        /// </summary>
        protected IPasswordStrategy PasswordStrategy
        {
            get
            {
                if (_passwordStrategy == null)
                {
                    _passwordStrategy = DependencyResolver.Current.GetService<IPasswordStrategy>();
                    if (_passwordStrategy == null)
                        throw new InvalidOperationException(
                            "You need to assign a locator to the ServiceLocator property and it should be able to lookup IPasswordStrategy.");
                }
                return _passwordStrategy;
            }
        }

        /// <summary>
        /// Gets password policy
        /// </summary>
        protected IPasswordPolicy PasswordPolicy
        {
            get
            {
                if (_passwordPolicy == null)
                {
                    _passwordPolicy = DependencyResolver.Current.GetService<IPasswordPolicy>();
                    if (_passwordPolicy == null)
                        throw new InvalidOperationException(
                            "You need to add an IPasswordPolicy implementation to your IoC container.");
                }

                return _passwordPolicy;
            }
        }

        /// <summary>
        /// Gets a brief, friendly description suitable for display in administrative tools or other user interfaces (UIs).
        /// </summary>
        /// <returns>A brief, friendly description suitable for display in administrative tools or other UIs.</returns>
        public override string Description
        {
            get { return "A more friendly membership provider."; }
        }

        /// <summary>
        /// Indicates whether the membership provider is configured to allow users to retrieve their passwords.
        /// </summary>
        /// <returns>
        /// true if the membership provider is configured to support password retrieval; otherwise, false. The default is false.
        /// </returns>
        public override bool EnablePasswordRetrieval
        {
            get { return PasswordStrategy.IsPasswordsDecryptable && PasswordPolicy.IsPasswordRetrievalEnabled; }
        }

        /// <summary>
        /// Indicates whether the membership provider is configured to allow users to reset their passwords.
        /// </summary>
        /// <returns>
        /// true if the membership provider supports password reset; otherwise, false. The default is true.
        /// </returns>
        public override bool EnablePasswordReset
        {
            get { return PasswordPolicy.IsPasswordResetEnabled; }
        }

        /// <summary>
        /// Gets a value indicating whether the membership provider is configured to require the user to answer a password question for password reset and retrieval.
        /// </summary>
        /// <returns>
        /// true if a password answer is required for password reset and retrieval; otherwise, false. The default is true.
        /// </returns>
        public override bool RequiresQuestionAndAnswer
        {
            get { return PasswordPolicy.IsPasswordQuestionRequired; }
        }

        /// <summary>
        /// The name of the application using the custom membership provider.
        /// </summary>
        /// <returns>
        /// The name of the application using the custom membership provider.
        /// </returns>
        public override string ApplicationName { get; set; }

        /// <summary>
        /// Gets the number of invalid password or password-answer attempts allowed before the membership user is locked out.
        /// </summary>
        /// <returns>
        /// The number of invalid password or password-answer attempts allowed before the membership user is locked out.
        /// </returns>
        public override int MaxInvalidPasswordAttempts
        {
            get { return PasswordPolicy.MaxInvalidPasswordAttempts; }
        }

        /// <summary>
        /// Gets the number of minutes in which a maximum number of invalid password or password-answer attempts are allowed before the membership user is locked out.
        /// </summary>
        /// <returns>
        /// The number of minutes in which a maximum number of invalid password or password-answer attempts are allowed before the membership user is locked out.
        /// </returns>
        public override int PasswordAttemptWindow
        {
            get { return PasswordPolicy.PasswordAttemptWindow; }
        }

        /// <summary>
        /// Gets a value indicating whether the membership provider is configured to require a unique e-mail address for each user name.
        /// </summary>
        /// <returns>
        /// true if the membership provider requires a unique e-mail address; otherwise, false. The default is true.
        /// </returns>
        public override bool RequiresUniqueEmail
        {
            get { return AccountRepository.IsUniqueEmailRequired; }
        }

        /// <summary>
        /// Gets a value indicating the format for storing passwords in the membership data store.
        /// </summary>
        /// <returns>
        /// One of the <see cref="T:System.Web.Security.MembershipPasswordFormat"/> values indicating the format for storing passwords in the data store.
        /// </returns>
        public override MembershipPasswordFormat PasswordFormat
        {
            get { return PasswordStrategy.PasswordFormat; }
        }

        /// <summary>
        /// Gets the minimum length required for a password.
        /// </summary>
        /// <returns>
        /// The minimum length required for a password. 
        /// </returns>
        public override int MinRequiredPasswordLength
        {
            get { return PasswordPolicy.PasswordMinimumLength; }
        }

        /// <summary>
        /// Gets the minimum number of special characters that must be present in a valid password.
        /// </summary>
        /// <returns>
        /// The minimum number of special characters that must be present in a valid password.
        /// </returns>
        public override int MinRequiredNonAlphanumericCharacters
        {
            get { return PasswordPolicy.MinRequiredNonAlphanumericCharacters; }
        }

        /// <summary>
        /// Gets the regular expression used to evaluate a password.
        /// </summary>
        /// <returns>
        /// A regular expression used to evaluate a password.
        /// </returns>
        public override string PasswordStrengthRegularExpression
        {
            get { return PasswordPolicy.PasswordStrengthRegularExpression; }
        }

        /// <summary>
        /// Adds a new membership user to the data source.
        /// </summary>
        /// <returns>
        /// A <see cref="T:System.Web.Security.MembershipUser"/> object populated with the information for the newly created user.
        /// </returns>
        /// <param name="username">The user name for the new user. </param><param name="password">The password for the new user. </param><param name="email">The e-mail address for the new user.</param><param name="passwordQuestion">The password question for the new user.</param><param name="passwordAnswer">The password answer for the new user</param><param name="isApproved">Whether or not the new user is approved to be validated.</param><param name="providerUserKey">The unique identifier from the membership data source for the user.</param><param name="status">A <see cref="T:System.Web.Security.MembershipCreateStatus"/> enumeration value indicating whether the user was created successfully.</param>
        public override MembershipUser CreateUser(string username, string password, string email,
                                                  string passwordQuestion, string passwordAnswer, bool isApproved,
                                                  object providerUserKey, out MembershipCreateStatus status)
        {
            var account = InternalCreateAccount(username, password, email, providerUserKey, isApproved, out status);
            if (status == MembershipCreateStatus.Success)
                return CloneUser(account);

            return null;
        }

        private IMembershipAccount InternalCreateAccount(string username, string password, string email,
                                                         object providerUserKey,
                                                         bool isApproved, out MembershipCreateStatus status)
        {
            if (AccountRepository.IsUniqueEmailRequired && AccountRepository.GetUserNameByEmail(email) != null)
            {
                status = MembershipCreateStatus.DuplicateEmail;
                return null;
            }

            if (AccountRepository.Get(username) != null)
            {
                status = MembershipCreateStatus.DuplicateUserName;
                return null;
            }

            try
            {
                ValidatePassword(username, password);
            }
            catch
            {
                // not the smoothest approach, but the best 
                // considering the inconsistent password failure handling.
                status = MembershipCreateStatus.InvalidPassword;
                return null;
            }

            var account = AccountRepository.Create(providerUserKey, ApplicationName, username, email);
            var passwordInfo = new AccountPasswordInfo(username, password);
            account.Password = PasswordStrategy.Encrypt(passwordInfo);
            account.PasswordSalt = passwordInfo.PasswordSalt;
            account.IsConfirmed = isApproved;
            if (!isApproved)
            {
                account.ConfirmationToken = GenerateToken();
            }

            status = AccountRepository.Register(account);

            return account;
        }

        private static string GenerateToken()
        {
            using (var prng = new RNGCryptoServiceProvider())
            {
                return GenerateToken(prng);
            }
        }

        internal static string GenerateToken(RandomNumberGenerator generator)
        {
            var tokenBytes = new byte[TokenSizeInBytes];
            generator.GetBytes(tokenBytes);
            return HttpServerUtility.UrlTokenEncode(tokenBytes);
        }

        /// <summary>
        /// Create a membershipuser from an membership account.
        /// </summary>
        /// <param name="account">The account.</param>
        /// <returns>Created user</returns>
        protected MembershipUser CloneUser(IMembershipAccount account)
        {
            return new MembershipUser(Name, account.UserName, account.ProviderUserKey, account.Email,
                                      account.PasswordQuestion, account.Comment, account.IsConfirmed,
                                      account.IsLockedOut, account.CreatedAt, account.LastLoginAt,
                                      account.LastActivityAt, account.LastPasswordChangeAt, account.LastLockedOutAt);
        }

        /// <summary>
        /// Processes a request to update the password question and answer for a membership user.
        /// </summary>
        /// <returns>
        /// true if the password question and answer are updated successfully; otherwise, false.
        /// </returns>
        /// <param name="username">The user to change the password question and answer for. </param><param name="password">The password for the specified user. </param><param name="newPasswordQuestion">The new password question for the specified user. </param><param name="newPasswordAnswer">The new password answer for the specified user. </param>
        public override bool ChangePasswordQuestionAndAnswer(string username, string password,
                                                             string newPasswordQuestion, string newPasswordAnswer)
        {
            var account = AccountRepository.Get(username);
            if (account == null)
                return false;

            var info = new AccountPasswordInfo(username, account.Password)
                {
                    PasswordSalt = account.PasswordSalt
                };
            if (PasswordStrategy.Compare(info, password))
                return false;

            account.PasswordQuestion = newPasswordAnswer;
            account.PasswordAnswer = newPasswordAnswer;
            AccountRepository.Update(account, MembershipEventType.ChangePasswordQuestionAndAnswer);
            return true;
        }

        /// <summary>
        /// Gets the password for the specified user name from the data source.
        /// </summary>
        /// <returns>
        /// The password for the specified user name.
        /// </returns>
        /// <param name="username">The user to retrieve the password for. </param><param name="answer">The password answer for the user. </param>
        public override string GetPassword(string username, string answer)
        {
            if (!PasswordPolicy.IsPasswordRetrievalEnabled || !PasswordStrategy.IsPasswordsDecryptable)
                throw new ProviderException("Password retrieval is not supported");

            var account = AccountRepository.Get(username);
            if (!account.PasswordAnswer.Equals(answer, StringComparison.OrdinalIgnoreCase))
                throw new MembershipPasswordException("Answer to Password question was incorrect.");

            return PasswordStrategy.Decrypt(account.Password);
        }

        /// <summary>
        /// Processes a request to update the password for a membership user.
        /// </summary>
        /// <returns>
        /// true if the password was updated successfully; otherwise, false.
        /// </returns>
        /// <param name="username">The user to update the password for. </param><param name="oldPassword">The current password for the specified user. </param><param name="newPassword">The new password for the specified user. </param>
        public override bool ChangePassword(string username, string oldPassword, string newPassword)
        {
            var account = AccountRepository.Get(username);
            var pwInfo = account.CreatePasswordInfo();
            if (!PasswordStrategy.Compare(pwInfo, oldPassword))
                return false;

            ValidatePassword(username, newPassword);

            account.Password = newPassword;
            pwInfo = account.CreatePasswordInfo();
            account.Password = PasswordStrategy.Encrypt(pwInfo);
            AccountRepository.Update(account, MembershipEventType.ChangePassword);
            return true;
        }

        /// <summary>
        /// Resets a user's password to a new, automatically generated password.
        /// </summary>
        /// <returns>
        /// The new password for the specified user.
        /// </returns>
        /// <param name="username">The user to reset the password for. </param><param name="answer">The password answer for the specified user. </param>
        public override string ResetPassword(string username, string answer)
        {
            if (!PasswordPolicy.IsPasswordResetEnabled)
                throw new NotSupportedException("Password reset is not supported.");

            var user = AccountRepository.Get(username);
            if (PasswordPolicy.IsPasswordQuestionRequired && answer == null)
                throw new MembershipPasswordException("Password answer is empty and question/answer is required.");

            if (!user.PasswordAnswer.Equals(answer, StringComparison.OrdinalIgnoreCase))
                return null;

            var newPassword = PasswordStrategy.GeneratePassword(PasswordPolicy);

            ValidatePassword(username, newPassword);

            var info = new AccountPasswordInfo(username, newPassword);
            user.Password = PasswordStrategy.Encrypt(info);
            user.PasswordSalt = info.PasswordSalt;
            AccountRepository.Update(user, MembershipEventType.ResetPassword);
            return newPassword;
        }

        private void ValidatePassword(string username, string clearTextPassword)
        {
            if (!PasswordStrategy.IsValid(clearTextPassword, PasswordPolicy))
                throw new MembershipPasswordException("Password failed validation");

            var args = new ValidatePasswordEventArgs(username, clearTextPassword, false);
            OnValidatingPassword(args);
            if (args.FailureInformation != null)
                throw args.FailureInformation;
        }

        /// <summary>
        /// Updates information about a user in the data source.
        /// </summary>
        /// <param name="user">A <see cref="T:System.Web.Security.MembershipUser"/> object that represents the user to update and the updated information for the user. </param>
        public override void UpdateUser(MembershipUser user)
        {
            var account = AccountRepository.Get(user.UserName);
            Merge(user, account);
            AccountRepository.Update(account);
        }

        private void Merge(MembershipUser user, IMembershipAccount account)
        {
            account.Comment = user.Comment;
            account.IsConfirmed = user.IsApproved;
            account.Email = user.Email;
            account.PasswordQuestion = user.PasswordQuestion;
            account.IsLockedOut = user.IsLockedOut;
            //account.IsOnline = user.IsOnline;
            account.LastActivityAt = user.LastActivityDate;
            account.LastLockedOutAt = user.LastLockoutDate;
            account.LastPasswordChangeAt = user.LastPasswordChangedDate;
            account.ProviderUserKey = user.ProviderUserKey;
            account.UserName = user.UserName;
        }

        /// <summary>
        /// Verifies that the specified user name and password exist in the data source.
        /// </summary>
        /// <returns>
        /// true if the specified username and password are valid; otherwise, false.
        /// </returns>
        /// <param name="username">The name of the user to validate. </param><param name="password">The password for the specified user. </param>
        public override bool ValidateUser(string username, string password)
        {
            var account = AccountRepository.Get(username);
            if (account == null || account.IsLockedOut)
                return false;

            var passwordInfo = account.CreatePasswordInfo();
            var validated = PasswordStrategy.Compare(passwordInfo, password);
            if (validated)
            {
                account.LastLoginAt = DateTime.Now;
                account.FailedPasswordWindowStartedAt = DateTime.MinValue;
                account.FailedPasswordWindowAttemptCount = 0;
                AccountRepository.Update(account, MembershipEventType.UserValidated);
                return true;
            }
            else
            {
                if (account.FailedPasswordWindowAttemptCount > PasswordPolicy.MaxInvalidPasswordAttempts)
                {
                    LockUser(account);
                }
                else
                {
                    account.FailedPasswordWindowAttemptCount += 1;
                    if (account.FailedPasswordWindowStartedAt == DateTime.MinValue)
                    {
                        account.FailedPasswordAnswerWindowStartedAt = DateTime.Now;
                    }
                    AccountRepository.Update(account, MembershipEventType.FailedLogin);

                    //if (DateTime.Now.Subtract(user.FailedPasswordAnswerWindowStartedAt).TotalMinutes >
                    //         PasswordPolicy.PasswordAttemptWindow)
                    //{
                    //    user.IsLockedOut = true;
                    //    user.LastLockedOutAt = DateTime.Now;
                    //    AccountRepository.Update(user, MembershipEventType.LockUser);
                    //}
                }
            }

            return false;
        }

        private void LockUser(IMembershipAccount account)
        {
            account.IsLockedOut = true;
            account.LastLockedOutAt = DateTime.Now;
            AccountRepository.Update(account, MembershipEventType.LockUser);
        }

        /// <summary>
        /// Clears a lock so that the membership user can be validated.
        /// </summary>
        /// <returns>
        /// true if the membership user was successfully unlocked; otherwise, false.
        /// </returns>
        /// <param name="userName">The membership user whose lock status you want to clear.</param>
        public override bool UnlockUser(string userName)
        {
            var user = AccountRepository.Get(userName);
            if (user == null)
                return false;

            user.IsLockedOut = false;
            user.FailedPasswordAnswerWindowAttemptCount = 0;
            user.FailedPasswordAnswerWindowStartedAt = DateTime.MinValue;
            user.FailedPasswordWindowAttemptCount = 0;
            user.FailedPasswordWindowStartedAt = DateTime.MinValue;
            AccountRepository.Update(user, MembershipEventType.UnlockUser);
            return true;
        }

        /// <summary>
        /// Gets user information from the data source based on the unique identifier for the membership user. Provides an option to update the last-activity date/time stamp for the user.
        /// </summary>
        /// <returns>
        /// A <see cref="T:System.Web.Security.MembershipUser"/> object populated with the specified user's information from the data source.
        /// </returns>
        /// <param name="providerUserKey">The unique identifier for the membership user to get information for.</param><param name="userIsOnline">true to update the last-activity date/time stamp for the user; false to return user information without updating the last-activity date/time stamp for the user.</param>
        public override MembershipUser GetUser(object providerUserKey, bool userIsOnline)
        {
            var user = AccountRepository.GetByProviderKey(providerUserKey);
            if (user == null)
                return null;

            UpdateOnlineState(userIsOnline, user);

            return CloneUser(user);
        }

        private void UpdateOnlineState(bool userIsOnline, IMembershipAccount user)
        {
            if (userIsOnline)
                return;

            user.LastActivityAt = DateTime.Now;
            //user.IsOnline = true;
            AccountRepository.Update(user, MembershipEventType.UpdateOnlineState);
        }

        /// <summary>
        /// Gets information from the data source for a user. Provides an option to update the last-activity date/time stamp for the user.
        /// </summary>
        /// <returns>
        /// A <see cref="T:System.Web.Security.MembershipUser"/> object populated with the specified user's information from the data source.
        /// </returns>
        /// <param name="username">The name of the user to get information for. 
        /// </param>
        /// <param name="userIsOnline">true to update the last-activity date/time stamp for the user; 
        /// false to return user information without updating the last-activity date/time stamp for the user. 
        /// </param>
        public override MembershipUser GetUser(string username, bool userIsOnline)
        {
            var user = AccountRepository.Get(username);
            if (user == null)
                return null;

            UpdateOnlineState(userIsOnline, user);

            return CloneUser(user);
        }


        /// <summary>
        /// Gets the user name associated with the specified e-mail address.
        /// </summary>
        /// <returns>
        /// The user name associated with the specified e-mail address. If no match is found, return null.
        /// </returns>
        /// <param name="email">The e-mail address to search for. </param>
        public override string GetUserNameByEmail(string email)
        {
            return AccountRepository.GetUserNameByEmail(email);
        }

        /// <summary>
        /// Removes a user from the membership data source. 
        /// </summary>
        /// <returns>
        /// true if the user was successfully deleted; otherwise, false.
        /// </returns>
        /// <param name="username">The name of the user to delete.</param><param name="deleteAllRelatedData">true to delete data related to the user from the database; false to leave data related to the user in the database.</param>
        public override bool DeleteUser(string username, bool deleteAllRelatedData)
        {
            return AccountRepository.Delete(username, deleteAllRelatedData);
        }

        /// <summary>
        /// Gets a collection of all the users in the data source in pages of data.
        /// </summary>
        /// <returns>
        /// A <see cref="T:System.Web.Security.MembershipUserCollection"/> collection that contains a page of <paramref name="pageSize"/><see cref="T:System.Web.Security.MembershipUser"/> objects beginning at the page specified by <paramref name="pageIndex"/>.
        /// </returns>
        /// <param name="pageIndex">The index of the page of results to return. <paramref name="pageIndex"/> is zero-based.</param><param name="pageSize">The size of the page of results to return.</param><param name="totalRecords">The total number of matched users.</param>
        public override MembershipUserCollection GetAllUsers(int pageIndex, int pageSize, out int totalRecords)
        {
            var users = AccountRepository.FindAll(pageIndex, pageSize, out totalRecords);
            return CloneUsers(users);
        }

        private MembershipUserCollection CloneUsers(IEnumerable<IMembershipAccount> users)
        {
            var members = new MembershipUserCollection();
            foreach (var user in users)
            {
                members.Add(CloneUser(user));
            }
            return members;
        }

        /// <summary>
        /// Gets the number of users currently accessing the application.
        /// </summary>
        /// <returns>
        /// The number of users currently accessing the application.
        /// </returns>
        public override int GetNumberOfUsersOnline()
        {
            return AccountRepository.GetNumberOfUsersOnline();
        }

        /// <summary>
        /// Gets a collection of membership users where the user name contains the specified user name to match.
        /// </summary>
        /// <returns>
        /// A <see cref="T:System.Web.Security.MembershipUserCollection"/> collection that contains a page of <paramref name="pageSize"/><see cref="T:System.Web.Security.MembershipUser"/> objects beginning at the page specified by <paramref name="pageIndex"/>.
        /// </returns>
        /// <param name="usernameToMatch">The user name to search for.</param><param name="pageIndex">The index of the page of results to return. <paramref name="pageIndex"/> is zero-based.</param><param name="pageSize">The size of the page of results to return.</param><param name="totalRecords">The total number of matched users.</param>
        public override MembershipUserCollection FindUsersByName(string usernameToMatch, int pageIndex, int pageSize,
                                                                 out int totalRecords)
        {
            var users = AccountRepository.FindByUserName(usernameToMatch, pageIndex, pageSize,
                                                         out totalRecords);
            return CloneUsers(users);
        }


        /// <summary>
        /// Gets a collection of membership users where the e-mail address contains the specified e-mail address to match.
        /// </summary>
        /// <returns>
        /// A <see cref="T:System.Web.Security.MembershipUserCollection"/> collection that contains a page of <paramref name="pageSize"/><see cref="T:System.Web.Security.MembershipUser"/> objects beginning at the page specified by <paramref name="pageIndex"/>.
        /// </returns>
        /// <param name="emailToMatch">The e-mail address to search for.</param><param name="pageIndex">The index of the page of results to return. <paramref name="pageIndex"/> is zero-based.</param><param name="pageSize">The size of the page of results to return.</param><param name="totalRecords">The total number of matched users.</param>
        public override MembershipUserCollection FindUsersByEmail(string emailToMatch, int pageIndex, int pageSize,
                                                                  out int totalRecords)
        {
            var users = AccountRepository.FindByEmail(emailToMatch, pageIndex, pageSize,
                                                      out totalRecords);
            return CloneUsers(users);
        }

        public override DateTime GetCreateDate(string userName)
        {
            var user = AccountRepository.Get(userName);
            if (user == null)
                return DateTime.MinValue;

            return user.CreatedAt;
        }

        public override DateTime GetLastPasswordFailureDate(string userName)
        {
            var user = AccountRepository.Get(userName);
            if (user == null)
                return DateTime.MinValue;

            return user.FailedPasswordWindowStartedAt;
        }

        public override DateTime GetPasswordChangedDate(string userName)
        {
            var user = AccountRepository.Get(userName);
            if (user == null)
                return DateTime.MinValue;

            return user.LastPasswordChangeAt;
        }

        public override int GetPasswordFailuresSinceLastSuccess(string userName)
        {
            var user = AccountRepository.Get(userName);
            if (user == null)
                return 0;

            return user.FailedPasswordWindowAttemptCount;
        }

        public override bool IsConfirmed(string userName)
        {
            var user = AccountRepository.Get(userName);
            if (user == null)
                return false;

            return user.IsConfirmed;
        }

        public override bool ConfirmAccount(string accountConfirmationToken)
        {
            return ConfirmAccount(null, accountConfirmationToken);
        }

        public override bool ConfirmAccount(string userName, string accountConfirmationToken)
        {
            bool isConfirmed = false;

            if (userName == null)
            {
                userName = AccountRepository.GetUserNameByConfirmationToken(accountConfirmationToken);
            }

            var user = AccountRepository.Get(userName);
            if (user != null)
            {
                isConfirmed = user.IsConfirmed = true;
                AccountRepository.Update(user, MembershipEventType.ConfirmAccount);
            }

            return isConfirmed;
        }

        public override string CreateAccount(string userName, string password, bool requireConfirmationToken)
        {
            return CreateUserAndAccount(userName, password, requireConfirmationToken, null);
        }

        public override string CreateUserAndAccount(string userName, string password, bool requireConfirmation,
                                                    IDictionary<string, object> values)
        {
            string token = string.Empty;

            string email = values == null ? string.Empty : (string) values["Email"];

            MembershipCreateStatus status;
            var account = InternalCreateAccount(userName, password, email, null, !requireConfirmation, out status);
            if (status == MembershipCreateStatus.Success)
            {
                token = account.ConfirmationToken;
            }

            return token;
        }

        public override bool DeleteAccount(string userName)
        {
            return this.DeleteUser(userName, true);
        }

        public override string GeneratePasswordResetToken(string userName, int tokenExpirationInMinutesFromNow)
        {
            throw new NotImplementedException();
        }

        public override ICollection<WebMatrix.WebData.OAuthAccountData> GetAccountsForUser(string userName)
        {
            throw new NotImplementedException();
        }

        public override int GetUserIdFromPasswordResetToken(string token)
        {
            throw new NotImplementedException();
        }


        public override bool ResetPasswordWithToken(string token, string newPassword)
        {
            throw new NotImplementedException();
        }
    }
}