namespace WB.UI.Designer.Providers.CQRS.Accounts
{
    using System;
    using Ncqrs.Domain;

    using WB.UI.Designer.Providers.CQRS.Accounts.Events;
    using WB.UI.Shared.Web.MembershipProvider.Roles;

    public class AccountAR : AggregateRootMappedByConvention
    {
        #region [Constants]
        private bool isLockOut = false;
        #endregion

        #region [State]

        public void Apply(AccountRegistered @event) { }
        public void Apply(AccountConfirmed @event) { }
        public void Apply(AccountDeleted @event) {}
        public void Apply(AccountLocked @event)
        {
            isLockOut = true;
        }
        public void Apply(AccountUnlocked @event)
        {
            isLockOut = false;
        }
        public void Apply(AccountLoginFailed @event) { }
        public void Apply(AccountOnlineUpdated @event) { }
        public void Apply(AccountPasswordChanged @event) { }
        public void Apply(AccountPasswordQuestionAndAnswerChanged @event) { }
        public void Apply(AccountPasswordReset @event) { }
        public void Apply(AccountUpdated @event) { }
        public void Apply(AccountPasswordResetTokenChanged @event) { }
        public void Apply(UserLoggedIn @event) { }
        [Obsolete]
        public void Apply(AccountValidated @event) { }
        #endregion
        public void Apply(AccountRoleAdded @event) { }
        public void Apply(AccountRoleRemoved @event) { }

        public AccountAR()
        {
        }

        public AccountAR(string applicationName, string userName, string email, Guid accountId, string password, string passwordSalt,
            bool isConfirmed, string confirmationToken)
            : base(accountId)
        {
            this.ApplyEvent(
                new AccountRegistered
                {
                    ApplicationName = applicationName,
                    Email = email,
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

        public void AddRole(SimpleRoleEnum role)
        {
            this.ApplyEvent(new AccountRoleAdded { Role = role });
        }

        public void ChangeOnline()
        {
            this.ApplyEvent(new AccountOnlineUpdated { LastActivityAt = DateTime.UtcNow });
        }

        public void ChangePassword(string password)
        {
            this.ApplyEvent(new AccountPasswordChanged { Password = password, LastPasswordChangeAt = DateTime.UtcNow });
        }

        public void ChangePasswordQuestionAndAnswer(string passwordQuestion, string passwordAnswer)
        {
            this.ApplyEvent(
                new AccountPasswordQuestionAndAnswerChanged
                    {
                        PasswordAnswer = passwordAnswer, 
                        PasswordQuestion = passwordQuestion
                    });
        }

        public void ChangePasswordResetToken(string passwordResetToken, DateTime passwordResetExpirationDate)
        {
            this.ApplyEvent(
                new AccountPasswordResetTokenChanged
                    {
                        PasswordResetToken = passwordResetToken, 
                        PasswordResetExpirationDate = passwordResetExpirationDate
                    });
        }

        public void Confirm()
        {
            this.ApplyEvent(new AccountConfirmed());
        }

        public void Delete()
        {
            this.ApplyEvent(new AccountDeleted());
        }

        public void Lock()
        {
            this.ApplyEvent(new AccountLocked { LastLockedOutAt = DateTime.UtcNow });
        }

        public void Unlock()
        {
            this.ApplyEvent(new AccountUnlocked());
        }

        public void LoginFailed()
        {
            this.ApplyEvent(new AccountLoginFailed { FailedPasswordWindowStartedAt = DateTime.UtcNow });
        }
        
        public void RemoveRole(SimpleRoleEnum role)
        {
            this.ApplyEvent(new AccountRoleRemoved { Role = role });
        }

        public void ResetPassword(string password, string passwordSalt)
        {
            this.ApplyEvent(new AccountPasswordReset { Password = password, PasswordSalt = passwordSalt });
        }

        public void Update(string userName, bool isLockedOut, string passwordQuestion, string email, bool isConfirmed, string comment)
        {
            this.ApplyEvent(
                new AccountUpdated
                {
                    Comment = comment,
                    Email = email,
                    PasswordQuestion = passwordQuestion,
                    UserName = userName
                });
            
            if (this.isLockOut != isLockedOut)
            {
                if (isLockedOut)
                {
                    this.ApplyEvent(new AccountLocked());
                }
                else
                {
                    this.ApplyEvent(new AccountUnlocked());
                }
            }
        }

        public void Validate()
        {
            this.ApplyEvent(new UserLoggedIn { LastLoginAt = DateTime.UtcNow });
        }
    }
}