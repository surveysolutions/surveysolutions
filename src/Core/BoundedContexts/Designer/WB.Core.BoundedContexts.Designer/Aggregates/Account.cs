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

        public Guid Id => this.ProviderUserKey;
        public void SetId(Guid id) => this.ProviderUserKey = id;
        private void ApplyEvent(dynamic @event) => ((dynamic) this).Apply(@event);

        public void Apply(AccountRegistered @event)
        {
            this.UserName = GetNormalizedUserName(@event.UserName);
            this.Email = @event.Email;
            this.ConfirmationToken = @event.ConfirmationToken;
            this.ApplicationName = @event.ApplicationName;
            this.CreatedAt = @event.CreatedDate;
        }

        public void Apply(AccountConfirmed @event)
        {
            this.IsConfirmed = true;
        }

        public void Apply(AccountDeleted @event) {}

        public void Apply(AccountLocked @event)
        {
            this.IsLockedOut = true;
            this.LastLockedOutAt = @event.LastLockedOutAt;
        }

        public void Apply(AccountOnlineUpdated @event)
        {
            this.LastActivityAt = @event.LastActivityAt;
        }

        public void Apply(AccountPasswordChanged @event)
        {
            this.Password = @event.Password;
            this.LastPasswordChangeAt = @event.LastPasswordChangeAt;
        }

        public void Apply(AccountPasswordQuestionAndAnswerChanged @event)
        {
            this.PasswordAnswer = @event.PasswordAnswer;
            this.PasswordQuestion = @event.PasswordQuestion;
        }

        public void Apply(AccountPasswordReset @event)
        {
            this.PasswordSalt = @event.PasswordSalt;
            this.Password = @event.Password;
        }
        public void Apply(AccountUnlocked @event)
        {
            this.IsLockedOut = false;
        }

        public void Apply(AccountUpdated @event)
        {
            this.Comment = @event.Comment;
            this.Email = @event.Email;
            this.PasswordQuestion = @event.PasswordQuestion;
            this.UserName = GetNormalizedUserName(@event.UserName);
        }

        public void Apply(UserLoggedIn @event)
        {
            this.LastLoginAt = @event.LastLoginAt;
        }

        public void Apply(AccountRoleAdded @event)
        {
            this.SimpleRoles.Add(@event.Role);
        }

        public void Apply(AccountRoleRemoved @event)
        {
            this.SimpleRoles.Remove(@event.Role);
        }
        public void Apply(AccountLoginFailed @event) { }

        public void Apply(AccountPasswordResetTokenChanged @event)
        {
            this.PasswordResetToken = @event.PasswordResetToken;
            this.PasswordResetExpirationDate = @event.PasswordResetExpirationDate;
        }

        public Account()
        {
        }

        public void RegisterAccount(string applicationName, string userName, string email, Guid accountId, string password, string passwordSalt, bool isConfirmed, string confirmationToken)
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