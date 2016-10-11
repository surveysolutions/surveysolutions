using System;
using Ncqrs.Domain;
using WB.Core.BoundedContexts.Designer.Views.Account;
using WB.Core.Infrastructure.Aggregates;
using WB.UI.Designer.Providers.CQRS.Accounts.Events;
using WB.UI.Shared.Web.MembershipProvider.Roles;

namespace WB.Core.BoundedContexts.Designer.Aggregates
{
    public class AccountAR : IPlainAggregateRoot
    {
        private Guid? id;

        public Guid Id // TODO: TLK: make plain after Document inline
        {
            get { return this.id ?? this.Document.ProviderUserKey; }
            private set { this.id = value; }
        }

        public AccountDocument Document { get; set; }

        public void SetId(Guid id) => this.Id = id;
        private void ApplyEvent(dynamic @event) => ((dynamic) this).Apply(@event);

        public void Apply(AccountRegistered @event)
        {
            this.Document = new AccountDocument
            {
                ProviderUserKey = this.Id,
                UserName = GetNormalizedUserName(@event.UserName),
                Email = @event.Email,
                ConfirmationToken = @event.ConfirmationToken,
                ApplicationName = @event.ApplicationName,
                CreatedAt = @event.CreatedDate
            };
        }
        public void Apply(AccountConfirmed @event)
        {
            this.Document.IsConfirmed = true;
        }

        public void Apply(AccountDeleted @event)
        {
            this.Document = null;
        }
        public void Apply(AccountLocked @event)
        {
            this.Document.IsLockedOut = true;
            this.Document.LastLockedOutAt = @event.LastLockedOutAt;
        }

        public void Apply(AccountOnlineUpdated @event)
        {
            this.Document.LastActivityAt = @event.LastActivityAt;
        }

        public void Apply(AccountPasswordChanged @event)
        {
            this.Document.Password = @event.Password;
            this.Document.LastPasswordChangeAt = @event.LastPasswordChangeAt;
        }

        public void Apply(AccountPasswordQuestionAndAnswerChanged @event)
        {
            this.Document.PasswordAnswer = @event.PasswordAnswer;
            this.Document.PasswordQuestion = @event.PasswordQuestion;
        }

        public void Apply(AccountPasswordReset @event)
        {
            this.Document.PasswordSalt = @event.PasswordSalt;
            this.Document.Password = @event.Password;
        }
        public void Apply(AccountUnlocked @event)
        {
            this.Document.IsLockedOut = false;
        }

        public void Apply(AccountUpdated @event)
        {
            this.Document.Comment = @event.Comment;
            this.Document.Email = @event.Email;
            this.Document.PasswordQuestion = @event.PasswordQuestion;
            this.Document.UserName = GetNormalizedUserName(@event.UserName);
        }

        public void Apply(UserLoggedIn @event)
        {
            this.Document.LastLoginAt = @event.LastLoginAt;
        }

        public void Apply(AccountRoleAdded @event)
        {
            this.Document.SimpleRoles.Add(@event.Role);
        }

        public void Apply(AccountRoleRemoved @event)
        {
            this.Document.SimpleRoles.Remove(@event.Role);
        }
        public void Apply(AccountLoginFailed @event) { }

        public void Apply(AccountPasswordResetTokenChanged @event)
        {
            this.Document.PasswordResetToken = @event.PasswordResetToken;
            this.Document.PasswordResetExpirationDate = @event.PasswordResetExpirationDate;
        }

        public AccountAR()
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
            
            if (!this.Document.IsConfirmed && isConfirmed)
            {
                this.ApplyEvent(new AccountConfirmed());
            }

            if (this.Document.IsLockedOut != isLockedOut)
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

        private static string GetNormalizedUserName(string userName) => userName.ToLower();
    }
}