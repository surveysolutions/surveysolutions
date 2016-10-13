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
    public class Account : IPlainAggregateRoot, IAccountView
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
        private void ApplyEvent(dynamic @event) => ((dynamic) this).Apply(@event);

        public virtual void Apply(AccountRegistered @event)
        {
            this.UserName = GetNormalizedUserName(@event.UserName);
            this.Email = @event.Email;
            this.ConfirmationToken = @event.ConfirmationToken;
            this.ApplicationName = @event.ApplicationName;
            this.CreatedAt = @event.CreatedDate;
        }

        public virtual void Apply(AccountConfirmed @event)
        {
            this.IsConfirmed = true;
        }

        public virtual void Apply(AccountDeleted @event) {}

        public virtual void Apply(AccountLocked @event)
        {
            this.IsLockedOut = true;
            this.LastLockedOutAt = @event.LastLockedOutAt;
        }

        public virtual void Apply(AccountOnlineUpdated @event)
        {
            this.LastActivityAt = @event.LastActivityAt;
        }

        public virtual void Apply(AccountPasswordChanged @event)
        {
            this.Password = @event.Password;
            this.LastPasswordChangeAt = @event.LastPasswordChangeAt;
        }

        public virtual void Apply(AccountPasswordQuestionAndAnswerChanged @event)
        {
            this.PasswordAnswer = @event.PasswordAnswer;
            this.PasswordQuestion = @event.PasswordQuestion;
        }

        public virtual void Apply(AccountPasswordReset @event)
        {
            this.PasswordSalt = @event.PasswordSalt;
            this.Password = @event.Password;
        }
        public virtual void Apply(AccountUnlocked @event)
        {
            this.IsLockedOut = false;
        }

        public virtual void Apply(AccountUpdated @event)
        {
            this.Comment = @event.Comment;
            this.Email = @event.Email;
            this.PasswordQuestion = @event.PasswordQuestion;
            this.UserName = GetNormalizedUserName(@event.UserName);
        }

        public virtual void Apply(UserLoggedIn @event)
        {
            this.LastLoginAt = @event.LastLoginAt;
        }

        public virtual void Apply(AccountRoleAdded @event)
        {
            this.SimpleRoles.Add(@event.Role);
        }

        public virtual void Apply(AccountRoleRemoved @event)
        {
            this.SimpleRoles.Remove(@event.Role);
        }
        public virtual void Apply(AccountLoginFailed @event) { }

        public virtual void Apply(AccountPasswordResetTokenChanged @event)
        {
            this.PasswordResetToken = @event.PasswordResetToken;
            this.PasswordResetExpirationDate = @event.PasswordResetExpirationDate;
        }

        public Account()
        {
        }

        public virtual void RegisterAccount(string applicationName, string userName, string email, Guid accountId, string password, string passwordSalt, bool isConfirmed, string confirmationToken)
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

        public virtual void AddRole(SimpleRoleEnum role)
        {
            this.ApplyEvent(new AccountRoleAdded { Role = role });
        }

        public virtual void ChangeOnline()
        {
            this.ApplyEvent(new AccountOnlineUpdated { LastActivityAt = DateTime.UtcNow });
        }

        public virtual void ChangePassword(string password)
        {
            this.ApplyEvent(new AccountPasswordChanged { Password = password, LastPasswordChangeAt = DateTime.UtcNow });
        }

        public virtual void ChangePasswordQuestionAndAnswer(string passwordQuestion, string passwordAnswer)
        {
            this.ApplyEvent(
                new AccountPasswordQuestionAndAnswerChanged
                    {
                        PasswordAnswer = passwordAnswer, 
                        PasswordQuestion = passwordQuestion
                    });
        }

        public virtual void ChangePasswordResetToken(string passwordResetToken, DateTime passwordResetExpirationDate)
        {
            this.ApplyEvent(
                new AccountPasswordResetTokenChanged
                    {
                        PasswordResetToken = passwordResetToken, 
                        PasswordResetExpirationDate = passwordResetExpirationDate
                    });
        }

        public virtual void Confirm()
        {
            this.ApplyEvent(new AccountConfirmed());
        }

        public virtual void Delete()
        {
            this.ApplyEvent(new AccountDeleted());
        }

        public virtual void Lock()
        {
            this.ApplyEvent(new AccountLocked { LastLockedOutAt = DateTime.UtcNow });
        }

        public virtual void Unlock()
        {
            this.ApplyEvent(new AccountUnlocked());
        }

        public virtual void LoginFailed()
        {
            this.ApplyEvent(new AccountLoginFailed { FailedPasswordWindowStartedAt = DateTime.UtcNow });
        }
        
        public virtual void RemoveRole(SimpleRoleEnum role)
        {
            this.ApplyEvent(new AccountRoleRemoved { Role = role });
        }

        public virtual void ResetPassword(string password, string passwordSalt)
        {
            this.ApplyEvent(new AccountPasswordReset { Password = password, PasswordSalt = passwordSalt });
        }

        public virtual void Update(string userName, bool isLockedOut, string passwordQuestion, string email, bool isConfirmed, string comment)
        {
            this.ApplyEvent(
                new AccountUpdated
                {
                    Comment = comment,
                    Email = email,
                    PasswordQuestion = passwordQuestion,
                    UserName = userName
                });
            
            if (!this.IsConfirmed && isConfirmed)
            {
                this.ApplyEvent(new AccountConfirmed());
            }

            if (this.IsLockedOut != isLockedOut)
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