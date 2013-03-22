using System;
using System.Collections.Generic;
using WB.UI.Designer.Providers.CQRS.Accounts.Events;
using Ncqrs.Domain;
using WB.UI.Designer.Providers.Roles;

namespace WB.UI.Designer.Providers.CQRS.Accounts
{
    using WB.UI.Designer.Providers.CQRS.Accounts.Events;
    using WB.UI.Designer.Providers.Roles;

    public class AccountAR : AggregateRootMappedByConvention
    {
        #warning Roma, please deal with not used fields which I commented. TLK.

        #region [Fields]

        private string _applicationName;
        private string _email;
        private string _passwordQuestion;
        private string _passwordAnswer;
        private string _comment;
        private DateTime _lastLoginAt;
        //private bool _isConfirmed;
        private DateTime _lastPasswordChangeAt;
        //private bool _isLockedOut;
        //private bool _isOnline;
        private DateTime _lastLockedOutAt;
        private DateTime _failedPasswordWindowStartedAt;
        private int _failedPasswordWindowAttemptCount = 0;
        private DateTime _failedPasswordAnswerWindowStartedAt;
        //private int _failedPasswordAnswerWindowAttemptCount;
        //private DateTime _createdAt;
        private DateTime _lastActivityAt;
        private object _providerUserKey;
        private string _userName;
        private string _password;
        private string _passwordSalt;
        private string _confirmationToken;
        private List<SimpleRoleEnum> _roles;

        private string _passwordResetToken;

        private DateTime _passwordResetExpirationDate;

        #endregion

        public AccountAR()
        {
        }

        public AccountAR(string applicationName, string userName, string email,
                         object providerUserKey, string password,
                         string passwordSalt, bool isConfirmed, string confirmationToken)
        {
            Register(applicationName: applicationName, userName: userName, email: email,
                     providerUserKey: providerUserKey,
                     password: password, passwordSalt: passwordSalt, isConfirmed: isConfirmed,
                     confirmationToken: confirmationToken);
        }

        public void Confirm()
        {
            ApplyEvent(new AccountConfirmed());
        }

        public void Delete()
        {
            ApplyEvent(new AccountDeleted());
        }

        public void Lock()
        {
            ApplyEvent(new AccountLocked()
                {
                    LastLockedOutAt =  DateTime.UtcNow
                });
        }

        public void Unlock()
        {
            ApplyEvent(new AccountUnlocked());
        }

        public void ChangeOnline()
        {
            ApplyEvent(new AccountOnlineUpdated()
                {
                    LastActivityAt = DateTime.UtcNow
                });
        }

        public void ChangePassword(string password)
        {
            ApplyEvent(new AccountPasswordChanged()
                {
                    Password = password,
                    LastPasswordChangeAt = DateTime.UtcNow
                });
        }

        public void ChangePasswordQuestionAndAnswer(string passwordQuestion, string passwordAnswer)
        {
            ApplyEvent(new AccountPasswordQuestionAndAnswerChanged()
                {
                    PasswordAnswer = passwordAnswer,
                    PasswordQuestion = passwordQuestion
                });
        }

        public void ResetPassword(string password, string passwordSalt)
        {
            ApplyEvent(new AccountPasswordReset()
                {
                    Password = password,
                    PasswordSalt = passwordSalt
                });
        }

        public void Register(string applicationName, string userName, string email, object providerUserKey,
                             string password, string passwordSalt, bool isConfirmed, string confirmationToken)
        {
            ApplyEvent(
                 new AccountRegistered
                 {
                     ApplicationName = applicationName,
                     Email = email,
                     ProviderUserKey = providerUserKey,
                     UserName = userName,
                     ConfirmationToken = confirmationToken,
                     CreatedDate = DateTime.UtcNow
                 });

            ResetPassword(password: password, passwordSalt: passwordSalt);
            if (isConfirmed)
            {
                Confirm();
            }
        }

        public void Update(string userName, bool isLockedOut, string passwordQuestion, string email,
                           bool isConfirmed, string comment)
        {
            ApplyEvent(new AccountUpdated()
                {
                    Comment = comment,
                    Email = email,
                    PasswordQuestion = passwordQuestion,
                    UserName = userName
                });

            if (!isConfirmed)
            {
                ApplyEvent(new AccountConfirmed());
            }

            if (isLockedOut)
            {
                ApplyEvent(new AccountLocked());
            }
            else
            {
                ApplyEvent(new AccountUnlocked());
            }
        }

        public void Validate()
        {
            ApplyEvent(new AccountValidated()
                {
                    LastLoginAt = DateTime.UtcNow
                });
        }

        public void AddRole(SimpleRoleEnum role)
        {
            ApplyEvent(new AccountRoleAdded()
                {
                    Role = role
                });
        }

        public void RemoveRole(SimpleRoleEnum role)
        {
            ApplyEvent(new AccountRoleRemoved()
                {
                    Role = role
                });
        }

        public void LoginFailed()
        {
            ApplyEvent(new AccountLoginFailed()
                {
                    FailedPasswordWindowStartedAt = DateTime.UtcNow
                });
        }

        public void ChangePasswordResetToken(string passwordResetToken, DateTime passwordResetExpirationDate)
        {
            this.ApplyEvent(new AccountPasswordResetTokenChanged()
                                {
                                    PasswordResetToken = passwordResetToken,
                                    PasswordResetExpirationDate = passwordResetExpirationDate
                                });
        }

        public void OnChangePasswordResetToken(AccountPasswordResetTokenChanged @event)
        {
            _passwordResetToken = @event.PasswordResetToken;
            _passwordResetExpirationDate = @event.PasswordResetExpirationDate;
        }

        public void OnAccountConfirmed(AccountConfirmed @event)
        {
            //_isConfirmed = true;
        }

        public void OnAccountLocked(AccountLocked @event)
        {
            //_isLockedOut = true;
            _lastLockedOutAt = @event.LastLockedOutAt;
        }

        public void OnAccountOnlineUpdated(AccountOnlineUpdated @event)
        {
            _lastActivityAt = @event.LastActivityAt;
        }

        public void OnAccountPasswordChanged(AccountPasswordChanged @event)
        {
            _password = @event.Password;
            _lastPasswordChangeAt = @event.LastPasswordChangeAt;
        }

        public void OnAccountPasswordQuestionAndAnswerChanged(AccountPasswordQuestionAndAnswerChanged @event)
        {
            _passwordQuestion = @event.PasswordQuestion;
            _passwordAnswer = @event.PasswordAnswer;
        }

        public void OnAccountPasswordReset(AccountPasswordReset @event)
        {
            _password = @event.Password;
            _passwordSalt = @event.PasswordSalt;
        }

        public void OnAccountRegistered(AccountRegistered @event)
        {
            _applicationName = @event.ApplicationName;
            _userName = @event.UserName;
            _email = @event.Email;
            _providerUserKey = @event.ProviderUserKey;
            _confirmationToken = @event.ConfirmationToken;
            _roles = new List<SimpleRoleEnum>();
        }

        public void OnAccountUnlocked(AccountUnlocked @event)
        {
            //_isLockedOut = false;
            //_failedPasswordAnswerWindowAttemptCount = 0;
            _failedPasswordAnswerWindowStartedAt = DateTime.MinValue;
            _failedPasswordWindowAttemptCount = 0;
            _failedPasswordWindowStartedAt = DateTime.MinValue;
        }

        public void OnAccountUpdated(AccountUpdated @event)
        {
            _userName = @event.UserName;
            _passwordQuestion = @event.PasswordQuestion;
            _email = @event.Email;
            _comment = @event.Comment;
        }

        public void OnAccountValidated(AccountValidated @event)
        {
            _lastLoginAt = @event.LastLoginAt;
            _failedPasswordWindowStartedAt = DateTime.MinValue;
            _failedPasswordWindowAttemptCount = 0;
        }

        public void OnAccountRoleAdded(AccountRoleAdded @event)
        {
            _roles.Add(@event.Role);
        }

        public void OnAccountRoleRemoved(AccountRoleRemoved @event)
        {
            _roles.Remove(@event.Role);
        }

        public void OnAccountLoginFailed(AccountLoginFailed @event)
        {
            _failedPasswordWindowStartedAt = @event.FailedPasswordWindowStartedAt;
            _failedPasswordWindowAttemptCount += 1;
        }

        public void OnAccountDeleted(AccountDeleted @event)
        {
            
        }
    }
}