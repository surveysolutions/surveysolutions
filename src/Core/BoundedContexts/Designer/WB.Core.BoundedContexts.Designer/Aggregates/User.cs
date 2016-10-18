using System;
using System.Collections.Generic;
using System.Linq;
using WB.Core.BoundedContexts.Designer.Views.Account;
using WB.Core.GenericSubdomains.Portable;
using WB.Core.Infrastructure.Aggregates;
using WB.UI.Designer.Providers.CQRS.Accounts.Events;
using WB.UI.Shared.Web.MembershipProvider.Accounts;
using WB.UI.Shared.Web.MembershipProvider.Roles;

namespace WB.Core.BoundedContexts.Designer.Aggregates
{
    public class User : IPlainAggregateRoot, IAccountView
    {
        #region Properties

        public virtual string ApplicationName { get; set; }

        public virtual string Comment { get; set; }

        public virtual string ConfirmationToken { get; set; }

        public virtual DateTime CreatedAt { get; set; }

        public virtual string Email { get; set; }

        public virtual bool IsConfirmed { get; set; }

        public virtual bool IsLockedOut { get; set; }

        public virtual bool IsOnline { get; set; }

        public virtual DateTime LastActivityAt { get; set; }

        public virtual DateTime LastLockedOutAt { get; set; }

        public virtual DateTime LastLoginAt { get; set; }

        public virtual DateTime LastPasswordChangeAt { get; set; }

        public virtual string Password { get; set; }

        public virtual string PasswordAnswer { get; set; }

        public virtual string PasswordQuestion { get; set; }

        public virtual DateTime PasswordResetExpirationDate { get; set; }

        public virtual string PasswordResetToken { get; set; }

        public virtual string PasswordSalt { get; set; }

        public virtual string UserId { get; set; }

        public virtual Guid ProviderUserKey
        {
            get { return providerUserKey; }
            set
            {
                this.UserId = value.FormatGuid();
                providerUserKey = value;
            }
        }

        private Guid providerUserKey;

        public virtual string UserName { get; set; }

        public virtual ISet<SimpleRoleEnum> SimpleRoles { get; set; } = new HashSet<SimpleRoleEnum>();

        #endregion

        public virtual Guid Id => this.ProviderUserKey;
        public virtual void SetId(Guid id) => this.ProviderUserKey = id;

        public virtual void Register(string applicationName, string userName, string email, Guid accountId, string password, string passwordSalt, bool isConfirmed, string confirmationToken)
        {
            AccountRegistered @event = new AccountRegistered
            {
                ApplicationName = applicationName,
                Email = email,
                UserName = userName,
                ConfirmationToken = confirmationToken,
                CreatedDate = DateTime.UtcNow
            };
            this.UserName = GetNormalizedUserName(@event.UserName);
            this.Email = @event.Email;
            this.ConfirmationToken = @event.ConfirmationToken;
            this.ApplicationName = @event.ApplicationName;
            this.CreatedAt = @event.CreatedDate;

            this.ResetPassword(password: password, passwordSalt: passwordSalt);
            if (isConfirmed)
            {
                this.Confirm();
            }
        }

        public virtual void AddRole(SimpleRoleEnum role)
        {
            this.SimpleRoles.Add(new AccountRoleAdded { Role = role }.Role);
        }

        public virtual void ChangePassword(string password)
        {
            AccountPasswordChanged @event = new AccountPasswordChanged { Password = password, LastPasswordChangeAt = DateTime.UtcNow };
            this.Password = @event.Password;
            this.LastPasswordChangeAt = @event.LastPasswordChangeAt;
        }

        public virtual void ChangePasswordQuestionAndAnswer(string passwordQuestion, string passwordAnswer)
        {
            AccountPasswordQuestionAndAnswerChanged @event = new AccountPasswordQuestionAndAnswerChanged
            {
                PasswordAnswer = passwordAnswer, 
                PasswordQuestion = passwordQuestion
            };
            this.PasswordAnswer = @event.PasswordAnswer;
            this.PasswordQuestion = @event.PasswordQuestion;
        }

        public virtual void ChangePasswordResetToken(string passwordResetToken, DateTime passwordResetExpirationDate)
        {
            AccountPasswordResetTokenChanged @event = new AccountPasswordResetTokenChanged
            {
                PasswordResetToken = passwordResetToken, 
                PasswordResetExpirationDate = passwordResetExpirationDate
            };
            this.PasswordResetToken = @event.PasswordResetToken;
            this.PasswordResetExpirationDate = @event.PasswordResetExpirationDate;
        }

        public virtual void Confirm()
        {
            AccountConfirmed @event = new AccountConfirmed();
            this.IsConfirmed = true;
        }

        public virtual void Delete()
        {
            AccountDeleted @event = new AccountDeleted();
        }

        public virtual void Lock()
        {
            this.IsLockedOut = true;
            this.LastLockedOutAt = new AccountLocked { LastLockedOutAt = DateTime.UtcNow }.LastLockedOutAt;
        }

        public virtual void Unlock()
        {
            AccountUnlocked @event = new AccountUnlocked();
            this.IsLockedOut = false;
        }

        public virtual void LoginFailed()
        {
            AccountLoginFailed @event = new AccountLoginFailed { FailedPasswordWindowStartedAt = DateTime.UtcNow };
        }
        
        public virtual void RemoveRole(SimpleRoleEnum role)
        {
            this.SimpleRoles.Remove(new AccountRoleRemoved { Role = role }.Role);
        }

        public virtual void ResetPassword(string password, string passwordSalt)
        {
            AccountPasswordReset @event = new AccountPasswordReset { Password = password, PasswordSalt = passwordSalt };
            this.PasswordSalt = @event.PasswordSalt;
            this.Password = @event.Password;
        }

        public virtual void Update(string userName, bool isLockedOut, string passwordQuestion, string email, bool isConfirmed, string comment)
        {
            AccountUpdated event1 = new AccountUpdated
            {
                Comment = comment,
                Email = email,
                PasswordQuestion = passwordQuestion,
                UserName = userName
            };
            this.Comment = event1.Comment;
            this.Email = event1.Email;
            this.PasswordQuestion = event1.PasswordQuestion;
            this.UserName = GetNormalizedUserName(event1.UserName);

            if (!this.IsConfirmed && isConfirmed)
            {
                AccountConfirmed @event = new AccountConfirmed();
                this.IsConfirmed = true;
            }

            if (this.IsLockedOut != isLockedOut)
            {
                if (isLockedOut)
                {
                    this.IsLockedOut = true;
                    this.LastLockedOutAt = new AccountLocked().LastLockedOutAt;
                }
                else
                {
                    AccountUnlocked @event = new AccountUnlocked();
                    this.IsLockedOut = false;
                }
            }
        }

        public virtual bool IsInRole(string roleName)
        {
            return this.GetRoles().Contains(roleName);
        }

        public virtual IEnumerable<string> GetRoles()
        {
            return this.SimpleRoles.Select(x => Enum.GetName(typeof(SimpleRoleEnum), x));
        }

        private static string GetNormalizedUserName(string userName) => userName.ToLower();
    }
}