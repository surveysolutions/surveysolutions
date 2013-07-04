namespace WB.UI.Designer.Providers.CQRS.Accounts
{
    using System;
    using System.Collections.Generic;

    using Ncqrs.Domain;

    using WB.UI.Designer.Providers.CQRS.Accounts.Events;
    using WB.UI.Shared.Web.MembershipProvider.Roles;

    /// <summary>
    ///     The account aggregate root
    /// </summary>
    public class AccountAR : AggregateRootMappedByConvention
    {
        #region Fields

        /// <summary>
        ///     The _application name.
        /// </summary>
        private string _applicationName;

        /// <summary>
        ///     The _comment.
        /// </summary>
        private string _comment;

        /// <summary>
        ///     The _confirmation token.
        /// </summary>
        private string _confirmationToken;

        /// <summary>
        ///     The _created at.
        /// </summary>
        private DateTime _createdAt;

        /// <summary>
        ///     The _email.
        /// </summary>
        private string _email;

        /// <summary>
        ///     The _failed password answer window started at.
        /// </summary>
        private DateTime _failedPasswordAnswerWindowStartedAt;

        /// <summary>
        ///     The _failed password window attempt count.
        /// </summary>
        private int _failedPasswordWindowAttemptCount;

        /// <summary>
        ///     The _failed password window started at.
        /// </summary>
        private DateTime _failedPasswordWindowStartedAt;

        /// <summary>
        ///     The _last activity at.
        /// </summary>
        private DateTime _lastActivityAt;

        /// <summary>
        ///     The _last locked out at.
        /// </summary>
        private DateTime _lastLockedOutAt;

        /// <summary>
        ///     The _last login at.
        /// </summary>
        private DateTime _lastLoginAt;

        /// <summary>
        ///     The _last password change at.
        /// </summary>
        private DateTime _lastPasswordChangeAt;

        /// <summary>
        ///     The _password.
        /// </summary>
        private string _password;

        /// <summary>
        ///     The _password answer.
        /// </summary>
        private string _passwordAnswer;

        /// <summary>
        ///     The _password question.
        /// </summary>
        private string _passwordQuestion;

        /// <summary>
        ///     The _password reset expiration date.
        /// </summary>
        private DateTime _passwordResetExpirationDate;

        /// <summary>
        ///     The _password reset token.
        /// </summary>
        private string _passwordResetToken;

        /// <summary>
        ///     The _password salt.
        /// </summary>
        private string _passwordSalt;

        /// <summary>
        ///     The _provider user key.
        /// </summary>
        private object _providerUserKey;

        /// <summary>
        ///     The _roles.
        /// </summary>
        private List<SimpleRoleEnum> _roles;

        /// <summary>
        ///     The _user name.
        /// </summary>
        private string _userName;

        #endregion

        #region Constructors and Destructors

        /// <summary>
        ///     Initializes a new instance of the <see cref="AccountAR" /> class.
        /// </summary>
        public AccountAR()
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="AccountAR"/> class.
        /// </summary>
        /// <param name="applicationName">
        /// The application name.
        /// </param>
        /// <param name="userName">
        /// The user name.
        /// </param>
        /// <param name="email">
        /// The email.
        /// </param>
        /// <param name="providerUserKey">
        /// The provider user key.
        /// </param>
        /// <param name="password">
        /// The password.
        /// </param>
        /// <param name="passwordSalt">
        /// The password salt.
        /// </param>
        /// <param name="isConfirmed">
        /// The is confirmed.
        /// </param>
        /// <param name="confirmationToken">
        /// The confirmation token.
        /// </param>
        public AccountAR(
            string applicationName, 
            string userName, 
            string email, 
            object providerUserKey, 
            string password, 
            string passwordSalt, 
            bool isConfirmed, 
            string confirmationToken)
        {
            this.Register(
                applicationName: applicationName, 
                userName: userName, 
                email: email, 
                providerUserKey: providerUserKey, 
                password: password, 
                passwordSalt: passwordSalt, 
                isConfirmed: isConfirmed, 
                confirmationToken: confirmationToken);
        }

        #endregion

        #region Public Methods and Operators

        /// <summary>
        /// The add role.
        /// </summary>
        /// <param name="role">
        /// The role.
        /// </param>
        public void AddRole(SimpleRoleEnum role)
        {
            this.ApplyEvent(new AccountRoleAdded { Role = role });
        }

        /// <summary>
        ///     The change online.
        /// </summary>
        public void ChangeOnline()
        {
            this.ApplyEvent(new AccountOnlineUpdated { LastActivityAt = DateTime.UtcNow });
        }

        /// <summary>
        /// The change password.
        /// </summary>
        /// <param name="password">
        /// The password.
        /// </param>
        public void ChangePassword(string password)
        {
            this.ApplyEvent(new AccountPasswordChanged { Password = password, LastPasswordChangeAt = DateTime.UtcNow });
        }

        /// <summary>
        /// The change password question and answer.
        /// </summary>
        /// <param name="passwordQuestion">
        /// The password question.
        /// </param>
        /// <param name="passwordAnswer">
        /// The password answer.
        /// </param>
        public void ChangePasswordQuestionAndAnswer(string passwordQuestion, string passwordAnswer)
        {
            this.ApplyEvent(
                new AccountPasswordQuestionAndAnswerChanged
                    {
                        PasswordAnswer = passwordAnswer, 
                        PasswordQuestion = passwordQuestion
                    });
        }

        /// <summary>
        /// The change password reset token.
        /// </summary>
        /// <param name="passwordResetToken">
        /// The password reset token.
        /// </param>
        /// <param name="passwordResetExpirationDate">
        /// The password reset expiration date.
        /// </param>
        public void ChangePasswordResetToken(string passwordResetToken, DateTime passwordResetExpirationDate)
        {
            this.ApplyEvent(
                new AccountPasswordResetTokenChanged
                    {
                        PasswordResetToken = passwordResetToken, 
                        PasswordResetExpirationDate = passwordResetExpirationDate
                    });
        }

        /// <summary>
        ///     The confirm.
        /// </summary>
        public void Confirm()
        {
            this.ApplyEvent(new AccountConfirmed());
        }

        /// <summary>
        ///     The delete.
        /// </summary>
        public void Delete()
        {
            this.ApplyEvent(new AccountDeleted());
        }

        /// <summary>
        ///     The lock.
        /// </summary>
        public void Lock()
        {
            this.ApplyEvent(new AccountLocked { LastLockedOutAt = DateTime.UtcNow });
        }

        /// <summary>
        ///     The login failed.
        /// </summary>
        public void LoginFailed()
        {
            this.ApplyEvent(new AccountLoginFailed { FailedPasswordWindowStartedAt = DateTime.UtcNow });
        }

        /// <summary>
        /// The on account confirmed.
        /// </summary>
        /// <param name="event">
        /// The event.
        /// </param>
        public void OnAccountConfirmed(AccountConfirmed @event)
        {}

        /// <summary>
        /// The on account deleted.
        /// </summary>
        /// <param name="event">
        /// The event.
        /// </param>
        public void OnAccountDeleted(AccountDeleted @event)
        {
        }

        /// <summary>
        /// The on account locked.
        /// </summary>
        /// <param name="event">
        /// The event.
        /// </param>
        public void OnAccountLocked(AccountLocked @event)
        {
            this._lastLockedOutAt = @event.LastLockedOutAt;
        }

        /// <summary>
        /// The on account login failed.
        /// </summary>
        /// <param name="event">
        /// The event.
        /// </param>
        public void OnAccountLoginFailed(AccountLoginFailed @event)
        {
            this._failedPasswordWindowStartedAt = @event.FailedPasswordWindowStartedAt;
            this._failedPasswordWindowAttemptCount += 1;
        }

        /// <summary>
        /// The on account online updated.
        /// </summary>
        /// <param name="event">
        /// The event.
        /// </param>
        public void OnAccountOnlineUpdated(AccountOnlineUpdated @event)
        {
            this._lastActivityAt = @event.LastActivityAt;
        }

        /// <summary>
        /// The on account password changed.
        /// </summary>
        /// <param name="event">
        /// The event.
        /// </param>
        public void OnAccountPasswordChanged(AccountPasswordChanged @event)
        {
            this._password = @event.Password;
            this._lastPasswordChangeAt = @event.LastPasswordChangeAt;
        }

        /// <summary>
        /// The on account password question and answer changed.
        /// </summary>
        /// <param name="event">
        /// The event.
        /// </param>
        public void OnAccountPasswordQuestionAndAnswerChanged(AccountPasswordQuestionAndAnswerChanged @event)
        {
            this._passwordQuestion = @event.PasswordQuestion;
            this._passwordAnswer = @event.PasswordAnswer;
        }

        /// <summary>
        /// The on account password reset.
        /// </summary>
        /// <param name="event">
        /// The event.
        /// </param>
        public void OnAccountPasswordReset(AccountPasswordReset @event)
        {
            this._password = @event.Password;
            this._passwordSalt = @event.PasswordSalt;
        }

        /// <summary>
        /// The on account registered.
        /// </summary>
        /// <param name="event">
        /// The event.
        /// </param>
        public void OnAccountRegistered(AccountRegistered @event)
        {
            this._applicationName = @event.ApplicationName;
            this._userName = @event.UserName;
            this._email = @event.Email;
            this._providerUserKey = @event.ProviderUserKey;
            this._confirmationToken = @event.ConfirmationToken;
            this._createdAt = @event.CreatedDate;
            this._roles = new List<SimpleRoleEnum>();
        }

        /// <summary>
        /// The on account role added.
        /// </summary>
        /// <param name="event">
        /// The event.
        /// </param>
        public void OnAccountRoleAdded(AccountRoleAdded @event)
        {
            this._roles.Add(@event.Role);
        }

        /// <summary>
        /// The on account role removed.
        /// </summary>
        /// <param name="event">
        /// The event.
        /// </param>
        public void OnAccountRoleRemoved(AccountRoleRemoved @event)
        {
            this._roles.Remove(@event.Role);
        }

        /// <summary>
        /// The on account unlocked.
        /// </summary>
        /// <param name="event">
        /// The event.
        /// </param>
        public void OnAccountUnlocked(AccountUnlocked @event)
        {
            this._failedPasswordAnswerWindowStartedAt = DateTime.MinValue;
            this._failedPasswordWindowAttemptCount = 0;
            this._failedPasswordWindowStartedAt = DateTime.MinValue;
        }

        /// <summary>
        /// The on account updated.
        /// </summary>
        /// <param name="event">
        /// The event.
        /// </param>
        public void OnAccountUpdated(AccountUpdated @event)
        {
            this._userName = @event.UserName;
            this._passwordQuestion = @event.PasswordQuestion;
            this._email = @event.Email;
            this._comment = @event.Comment;
        }

        /// <summary>
        /// The on change password reset token.
        /// </summary>
        /// <param name="event">
        /// The event.
        /// </param>
        public void OnChangePasswordResetToken(AccountPasswordResetTokenChanged @event)
        {
            this._passwordResetToken = @event.PasswordResetToken;
            this._passwordResetExpirationDate = @event.PasswordResetExpirationDate;
        }

        /// <summary>
        /// The on user logged in.
        /// </summary>
        /// <param name="event">
        /// The event.
        /// </param>
        public void OnUserLoggedIn(UserLoggedIn @event)
        {
            this._lastLoginAt = @event.LastLoginAt;
            this._failedPasswordWindowStartedAt = DateTime.MinValue;
            this._failedPasswordWindowAttemptCount = 0;
        }

        /// <summary>
        /// The register.
        /// </summary>
        /// <param name="applicationName">
        /// The application name.
        /// </param>
        /// <param name="userName">
        /// The user name.
        /// </param>
        /// <param name="email">
        /// The email.
        /// </param>
        /// <param name="providerUserKey">
        /// The provider user key.
        /// </param>
        /// <param name="password">
        /// The password.
        /// </param>
        /// <param name="passwordSalt">
        /// The password salt.
        /// </param>
        /// <param name="isConfirmed">
        /// The is confirmed.
        /// </param>
        /// <param name="confirmationToken">
        /// The confirmation token.
        /// </param>
        public void Register(
            string applicationName, 
            string userName, 
            string email, 
            object providerUserKey, 
            string password, 
            string passwordSalt, 
            bool isConfirmed, 
            string confirmationToken)
        {
            this.ApplyEvent(
                new AccountRegistered
                    {
                        ApplicationName = applicationName, 
                        Email = email, 
                        ProviderUserKey = providerUserKey, 
                        UserName = userName, 
                        ConfirmationToken = confirmationToken, 
                        CreatedDate = DateTime.UtcNow
                    });

            this.ResetPassword(password: password, passwordSalt: passwordSalt);
            if (isConfirmed)
            {
                this.Confirm();
            }
        }

        /// <summary>
        /// The remove role.
        /// </summary>
        /// <param name="role">
        /// The role.
        /// </param>
        public void RemoveRole(SimpleRoleEnum role)
        {
            this.ApplyEvent(new AccountRoleRemoved { Role = role });
        }

        /// <summary>
        /// The reset password.
        /// </summary>
        /// <param name="password">
        /// The password.
        /// </param>
        /// <param name="passwordSalt">
        /// The password salt.
        /// </param>
        public void ResetPassword(string password, string passwordSalt)
        {
            this.ApplyEvent(new AccountPasswordReset { Password = password, PasswordSalt = passwordSalt });
        }

        /// <summary>
        ///     The unlock.
        /// </summary>
        public void Unlock()
        {
            this.ApplyEvent(new AccountUnlocked());
        }

        /// <summary>
        /// The update.
        /// </summary>
        /// <param name="userName">
        /// The user name.
        /// </param>
        /// <param name="isLockedOut">
        /// The is locked out.
        /// </param>
        /// <param name="passwordQuestion">
        /// The password question.
        /// </param>
        /// <param name="email">
        /// The email.
        /// </param>
        /// <param name="isConfirmed">
        /// The is confirmed.
        /// </param>
        /// <param name="comment">
        /// The comment.
        /// </param>
        public void Update(
            string userName, bool isLockedOut, string passwordQuestion, string email, bool isConfirmed, string comment)
        {
            this.ApplyEvent(
                new AccountUpdated
                    {
                        Comment = comment, 
                        Email = email, 
                        PasswordQuestion = passwordQuestion, 
                        UserName = userName
                    });

            if (isConfirmed)
            {
                this.ApplyEvent(new AccountConfirmed());
            }

            if (isLockedOut)
            {
                this.ApplyEvent(new AccountLocked());
            }
            else
            {
                this.ApplyEvent(new AccountUnlocked());
            }
        }

        /// <summary>
        ///     The validate.
        /// </summary>
        public void Validate()
        {
            this.ApplyEvent(new UserLoggedIn { LastLoginAt = DateTime.UtcNow });
        }

        [Obsolete]
        public void OnAccountValidated(AccountValidated @event)
        {
        }

        #endregion
    }
}