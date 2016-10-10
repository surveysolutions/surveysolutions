using System;
using Ncqrs.Domain;
using WB.Core.BoundedContexts.Designer.Views.Account;
using WB.UI.Designer.Providers.CQRS.Accounts.Events;
using WB.UI.Shared.Web.MembershipProvider.Roles;

namespace WB.Core.BoundedContexts.Designer.Aggregates
{
    public class AccountAR : AggregateRootMappedByConvention
    {
        private bool isLockOut = false;
        private bool isConfirmed = false;
        private AccountDocument document = null;

        public void Apply(AccountRegistered @event)
        {
            this.document = new AccountDocument
            {
                ProviderUserKey = this.EventSourceId,
                UserName = GetNormalizedUserName(@event.UserName),
                Email = @event.Email,
                ConfirmationToken = @event.ConfirmationToken,
                ApplicationName = @event.ApplicationName,
                CreatedAt = @event.CreatedDate
            };
        }
        public void Apply(AccountConfirmed @event)
        {
            this.isConfirmed = true;
            this.document.IsConfirmed = true;
        }

        public void Apply(AccountDeleted @event)
        {
            this.document = null;
        }
        public void Apply(AccountLocked @event)
        {
            this.isLockOut = true;
            this.document.IsLockedOut = true;
            this.document.LastLockedOutAt = @event.LastLockedOutAt;
        }

        public void Apply(AccountOnlineUpdated @event)
        {
            this.document.LastActivityAt = @event.LastActivityAt;
        }

        public void Apply(AccountPasswordChanged @event)
        {
            this.document.Password = @event.Password;
            this.document.LastPasswordChangeAt = @event.LastPasswordChangeAt;
        }

        public void Apply(AccountPasswordQuestionAndAnswerChanged @event)
        {
            this.document.PasswordAnswer = @event.PasswordAnswer;
            this.document.PasswordQuestion = @event.PasswordQuestion;
        }

        public void Apply(AccountPasswordReset @event)
        {
            this.document.PasswordSalt = @event.PasswordSalt;
            this.document.Password = @event.Password;
        }
        public void Apply(AccountUnlocked @event)
        {
            this.isLockOut = false;
            this.document.IsLockedOut = false;
        }

        public void Apply(AccountUpdated @event)
        {
            this.document.Comment = @event.Comment;
            this.document.Email = @event.Email;
            this.document.PasswordQuestion = @event.PasswordQuestion;
            this.document.UserName = GetNormalizedUserName(@event.UserName);
        }

        public void Apply(UserLoggedIn @event)
        {
            this.document.LastLoginAt = @event.LastLoginAt;
        }

        public void Apply(AccountRoleAdded @event)
        {
            this.document.SimpleRoles.Add(@event.Role);
        }

        public void Apply(AccountRoleRemoved @event)
        {
            this.document.SimpleRoles.Remove(@event.Role);
        }
        public void Apply(AccountLoginFailed @event) { }

        public void Apply(AccountPasswordResetTokenChanged @event)
        {
            this.document.PasswordResetToken = @event.PasswordResetToken;
            this.document.PasswordResetExpirationDate = @event.PasswordResetExpirationDate;
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
            
            if (!this.isConfirmed && isConfirmed)
            {
                this.ApplyEvent(new AccountConfirmed());
            }

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

        private static string GetNormalizedUserName(string userName) => userName.ToLower();
    }
}