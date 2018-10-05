using System;
using System.Collections.Generic;
using System.Configuration.Provider;
using System.Security.Cryptography;
using System.Web;
using System.Web.Security;
using WB.Core.BoundedContexts.Designer.Services.Accounts;
using WB.Core.GenericSubdomains.Portable;
using WB.Core.GenericSubdomains.Portable.ServiceLocation;
using WebMatrix.WebData;

namespace WB.Core.BoundedContexts.Designer.Implementation.Services.Accounts
{
    public class MembershipProvider : ExtendedMembershipProvider
    {
        private const int TokenSizeInBytes = 16;

        public override string ApplicationName { get; set; }

        public override string Description => "A more friendly membership provider.";

        public override bool EnablePasswordReset => this.PasswordPolicy.IsPasswordResetEnabled;

        public override bool EnablePasswordRetrieval => this.PasswordStrategy.IsPasswordsDecryptable && this.PasswordPolicy.IsPasswordRetrievalEnabled;

        public override int MaxInvalidPasswordAttempts => this.PasswordPolicy.MaxInvalidPasswordAttempts;

        public override int MinRequiredNonAlphanumericCharacters => this.PasswordPolicy.MinRequiredNonAlphanumericCharacters;

        public override int MinRequiredPasswordLength => this.PasswordPolicy.PasswordMinimumLength;

        public override int PasswordAttemptWindow => this.PasswordPolicy.PasswordAttemptWindow;

        public override MembershipPasswordFormat PasswordFormat => this.PasswordStrategy.PasswordFormat;

        public override string PasswordStrengthRegularExpression => this.PasswordPolicy.PasswordStrengthRegularExpression;

        public override bool RequiresQuestionAndAnswer => this.PasswordPolicy.IsPasswordQuestionRequired;

        public override bool RequiresUniqueEmail => this.AccountRepository.IsUniqueEmailRequired;

        protected IAccountRepository AccountRepository => ServiceLocator.Current.GetInstance<IAccountRepository>();

        protected IPasswordPolicy PasswordPolicy => ServiceLocator.Current.GetInstance<IPasswordPolicy>();

        protected IPasswordStrategy PasswordStrategy => ServiceLocator.Current.GetInstance<IPasswordStrategy>();

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

        public override bool ConfirmAccount(string accountConfirmationToken)
        {
            return this.ConfirmAccount(null, accountConfirmationToken);
        }

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

        public override string CreateAccount(string userName, string password, bool requireConfirmationToken)
        {
            return this.CreateUserAndAccount(userName, password, requireConfirmationToken, null);
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
            IMembershipAccount account = this.InternalCreateAccount(
                username, password, email, null, providerUserKey, isApproved, out status);
            if (status == MembershipCreateStatus.Success)
            {
                return this.CloneUser(account);
            }

            return null;
        }

        public override string CreateUserAndAccount(
            string userName, string password, bool requireConfirmation, IDictionary<string, object> values)
        {
            const string emailParameterName = "Email";
            const string providerUserKeyParameterName = "ProviderUserKey";
            const string fullNameParameterName = "FullName";

            string email = string.Empty;
            Guid? providerUserKey = null;
            string fullName = null;

            if (values.ContainsKey(emailParameterName))
                email = values[emailParameterName] as string;

            if (values.ContainsKey(fullNameParameterName))
                fullName = values[fullNameParameterName] as string;

            if (values.ContainsKey(providerUserKeyParameterName))
                providerUserKey = values[providerUserKeyParameterName] as Guid?;

            IMembershipAccount account = this.InternalCreateAccount(
                userName, password, email, fullName, providerUserKey, !requireConfirmation, out var status);

            return status == MembershipCreateStatus.Success
                ? account.ConfirmationToken
                : throw new MembershipCreateUserException(status);
        }

        public override bool DeleteAccount(string userName)
        {
            return this.DeleteUser(userName, true);
        }

        public override bool DeleteUser(string username, bool deleteAllRelatedData)
        {
            return this.AccountRepository.Delete(username);
        }

        public override MembershipUserCollection FindUsersByEmail(
            string emailToMatch, int pageIndex, int pageSize, out int totalRecords)
        {
            IEnumerable<IMembershipAccount> users = this.AccountRepository.FindByEmail(
                emailToMatch, pageIndex, pageSize, out totalRecords);
            return this.CloneUsers(users);
        }

        public override MembershipUserCollection FindUsersByName(
            string usernameToMatch, int pageIndex, int pageSize, out int totalRecords)
        {
            IEnumerable<IMembershipAccount> users = this.AccountRepository.FindByUserName(
                usernameToMatch, pageIndex, pageSize, out totalRecords);
            return this.CloneUsers(users);
        }

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

        public override ICollection<OAuthAccountData> GetAccountsForUser(string userName)
        {
            throw new NotImplementedException();
        }

        public override MembershipUserCollection GetAllUsers(int pageIndex, int pageSize, out int totalRecords)
        {
            IEnumerable<IMembershipAccount> users = this.AccountRepository.FindAll(
                pageIndex, pageSize, out totalRecords);
            return this.CloneUsers(users);
        }

        public override DateTime GetCreateDate(string userName)
        {
            IMembershipAccount user = this.AccountRepository.Get(userName);
            if (user == null)
            {
                return DateTime.MinValue;
            }

            return user.CreatedAt;
        }

        public override DateTime GetLastPasswordFailureDate(string userName)
        {
            return DateTime.MinValue;
        }

        public override int GetNumberOfUsersOnline()
        {
            return this.AccountRepository.GetNumberOfUsersOnline();
        }

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

        public override DateTime GetPasswordChangedDate(string userName)
        {
            IMembershipAccount user = this.AccountRepository.Get(userName);
            if (user == null)
            {
                return DateTime.MinValue;
            }

            return user.LastPasswordChangeAt;
        }

        public override int GetPasswordFailuresSinceLastSuccess(string userName)
        {
            return 0;
        }

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

        public override MembershipUser GetUser(string userNameOrEmail, bool userIsOnline)
        {
            IMembershipAccount user = this.AccountRepository.GetByNameOrEmail(userNameOrEmail);
            if (user == null)
            {
                return null;
            }

            this.UpdateOnlineState(userIsOnline, user);

            return this.CloneUser(user);
        }

        public override int GetUserIdFromPasswordResetToken(string token)
        {
            throw new NotImplementedException();
        }

        public override string GetUserNameByEmail(string email)
        {
            return this.AccountRepository.GetUserNameByEmail(email);
        }

        public override bool IsConfirmed(string userName)
        {
            IMembershipAccount user = this.AccountRepository.Get(userName);
            if (user == null)
            {
                return false;
            }

            return user.IsConfirmed;
        }

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

        public override bool UnlockUser(string userName)
        {
            IMembershipAccount user = this.AccountRepository.Get(userName);
            if (user == null)
            {
                return false;
            }

            user.IsLockedOut = false;
            this.AccountRepository.Update(user, MembershipEventType.UnlockUser);
            return true;
        }

        public override void UpdateUser(MembershipUser user)
        {
            IMembershipAccount account = this.AccountRepository.GetByProviderKey(user.ProviderUserKey);
            this.Merge((DesignerMembershipUser)user, account);
            this.AccountRepository.Update(account);
        }

        public override bool ValidateUser(string userNameOrEmail, string password)
        {
            IMembershipAccount account = this.AccountRepository.GetByNameOrEmail(userNameOrEmail);
            if (account == null)
            {
                return false;
            }

            AccountPasswordInfo passwordInfo = account.CreatePasswordInfo();
            return this.PasswordStrategy.Compare(passwordInfo, password);
        }

        internal static string GenerateToken(RandomNumberGenerator generator)
        {
            var tokenBytes = new byte[TokenSizeInBytes];
            generator.GetBytes(tokenBytes);
            return HttpServerUtility.UrlTokenEncode(tokenBytes);
        }

        protected DesignerMembershipUser CloneUser(IMembershipAccount account)
        {
            return new DesignerMembershipUser(
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
                account.LastLockedOutAt,
                account.CanImportOnHq,
                account.FullName);
        }

        private static string GenerateToken()
        {
            using (var prng = new RNGCryptoServiceProvider())
            {
                return GenerateToken(prng);
            }
        }

        private MembershipUserCollection CloneUsers(IEnumerable<IMembershipAccount> users)
        {
            var members = new MembershipUserCollection();
            foreach (IMembershipAccount user in users)
            {
                members.Add(this.CloneUser(user));
            }

            return members;
        }

        private IMembershipAccount InternalCreateAccount(string username,
            string password,
            string email,
            string fullName,
            object providerUserKey,
            bool isApproved,
            out MembershipCreateStatus status)
        {
            if (this.AccountRepository.IsUniqueEmailRequired && !string.IsNullOrEmpty(this.AccountRepository.GetUserNameByEmail(email)))
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
                providerUserKey, this.ApplicationName, username, email, fullName);
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

        private void Merge(DesignerMembershipUser user, IMembershipAccount account)
        {
            string userNameByEmail = this.AccountRepository.GetUserNameByEmail(user.Email);

            if (this.AccountRepository.IsUniqueEmailRequired && !string.IsNullOrEmpty(userNameByEmail) && userNameByEmail != account.UserName)
            {
                throw new ProviderException($"User with e-mail '{user.Email}' already exists. Please enter a different e-mail address.");
            }

            account.Comment = user.Comment;
            account.IsConfirmed = user.IsApproved;
            account.Email = user.Email;
            account.PasswordQuestion = user.PasswordQuestion;
            account.IsLockedOut = user.IsLockedOut;

            // account.IsOnline = user.IsOnline;
            account.LastActivityAt = user.LastActivityDate;
            account.LastLockedOutAt = user.LastLockoutDate;
            account.LastPasswordChangeAt = user.LastPasswordChangedDate;
            account.ProviderUserKey = Guid.Parse(user.ProviderUserKey.ToString());
            account.UserName = user.UserName;
            account.CanImportOnHq = user.CanImportOnHq;
            account.FullName = user.FullName;
        }

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
    }
}
