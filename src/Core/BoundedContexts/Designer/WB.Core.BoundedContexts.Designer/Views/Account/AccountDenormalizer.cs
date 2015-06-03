using System;
using System.Collections.Generic;
using Ncqrs.Eventing.ServiceModel.Bus;
using WB.Core.Infrastructure.EventBus;
using WB.Core.Infrastructure.ReadSide.Repository.Accessors;
using WB.UI.Designer.Providers.CQRS.Accounts.Events;
using WB.UI.Shared.Web.MembershipProvider.Roles;

namespace WB.Core.BoundedContexts.Designer.Views.Account
{
    internal class AccountDenormalizer : BaseDenormalizer, IEventHandler<AccountConfirmed>, 
                                       IEventHandler<AccountDeleted>, 
                                       IEventHandler<AccountLocked>, 
                                       IEventHandler<AccountOnlineUpdated>, 
                                       IEventHandler<AccountPasswordChanged>, 
                                       IEventHandler<AccountPasswordQuestionAndAnswerChanged>, 
                                       IEventHandler<AccountPasswordReset>, 
                                       IEventHandler<AccountRegistered>, 
                                       IEventHandler<AccountUnlocked>, 
                                       IEventHandler<AccountUpdated>, 
                                       IEventHandler<UserLoggedIn>, 
                                       IEventHandler<AccountRoleAdded>, 
                                       IEventHandler<AccountRoleRemoved>, 
                                       IEventHandler<AccountLoginFailed>, 
                                       IEventHandler<AccountPasswordResetTokenChanged>, 
                                       IEventHandler<AccountValidated>
    {
        private readonly IReadSideRepositoryWriter<AccountDocument> _accounts;

        public AccountDenormalizer(IReadSideRepositoryWriter<AccountDocument> accounts)
        {
            this._accounts = accounts;
        }

        public override object[] Writers
        {
            get { return new object[] { _accounts }; }
        }

        public void Handle(IPublishedEvent<AccountConfirmed> @event)
        {
            AccountDocument item = this._accounts.GetById(@event.EventSourceId);

            item.IsConfirmed = true;
            this._accounts.Store(item, @event.EventSourceId);
        }

        public void Handle(IPublishedEvent<AccountDeleted> @event)
        {
            this._accounts.Remove(@event.EventSourceId);
        }

        public void Handle(IPublishedEvent<AccountLocked> @event)
        {
            AccountDocument item = this._accounts.GetById(@event.EventSourceId);

            item.IsLockedOut = true;
            item.LastLockedOutAt = @event.Payload.LastLockedOutAt;
            this._accounts.Store(item, @event.EventSourceId);
        }

        public void Handle(IPublishedEvent<AccountOnlineUpdated> @event)
        {
            AccountDocument item = this._accounts.GetById(@event.EventSourceId);

            item.LastActivityAt = @event.Payload.LastActivityAt;
            this._accounts.Store(item, @event.EventSourceId);
        }

        public void Handle(IPublishedEvent<AccountPasswordChanged> @event)
        {
            AccountDocument item = this._accounts.GetById(@event.EventSourceId);

            item.Password = @event.Payload.Password;
            item.LastPasswordChangeAt = @event.Payload.LastPasswordChangeAt;
            this._accounts.Store(item, @event.EventSourceId);
        }

        public void Handle(IPublishedEvent<AccountPasswordQuestionAndAnswerChanged> @event)
        {
            AccountDocument item = this._accounts.GetById(@event.EventSourceId);

            item.PasswordAnswer = @event.Payload.PasswordAnswer;
            item.PasswordQuestion = @event.Payload.PasswordQuestion;
            this._accounts.Store(item, @event.EventSourceId);
        }

        public void Handle(IPublishedEvent<AccountPasswordReset> @event)
        {
            AccountDocument item = this._accounts.GetById(@event.EventSourceId);

            item.PasswordSalt = @event.Payload.PasswordSalt;
            item.Password = @event.Payload.Password;
            this._accounts.Store(item, @event.EventSourceId);
        }

        public void Handle(IPublishedEvent<AccountRegistered> @event)
        {
            this._accounts.Store(
                new AccountDocument
                    {
                        ProviderUserKey = @event.EventSourceId, 
                        UserName = GetNormalizedUserName(@event.Payload.UserName), 
                        Email = @event.Payload.Email, 
                        ConfirmationToken = @event.Payload.ConfirmationToken, 
                        ApplicationName = @event.Payload.ApplicationName, 
                        CreatedAt = @event.Payload.CreatedDate, 
                        FailedPasswordAnswerWindowAttemptCount = 0, 
                        FailedPasswordWindowAttemptCount = 0
                    }, 
                @event.EventSourceId);
        }

        public void Handle(IPublishedEvent<AccountUnlocked> @event)
        {
            AccountDocument item = this._accounts.GetById(@event.EventSourceId);

            item.IsLockedOut = false;
            item.FailedPasswordAnswerWindowAttemptCount = 0;
            item.FailedPasswordAnswerWindowStartedAt = DateTime.MinValue;
            item.FailedPasswordWindowAttemptCount = 0;
            item.FailedPasswordWindowStartedAt = DateTime.MinValue;
            this._accounts.Store(item, @event.EventSourceId);
        }

        public void Handle(IPublishedEvent<AccountUpdated> @event)
        {
            AccountDocument item = this._accounts.GetById(@event.EventSourceId);

            item.Comment = @event.Payload.Comment;
            item.Email = @event.Payload.Email;
            item.PasswordQuestion = @event.Payload.PasswordQuestion;
            item.UserName = GetNormalizedUserName(@event.Payload.UserName);
            this._accounts.Store(item, @event.EventSourceId);
        }

        public void Handle(IPublishedEvent<UserLoggedIn> @event)
        {
            AccountDocument item = this._accounts.GetById(@event.EventSourceId);

            item.LastLoginAt = @event.Payload.LastLoginAt;
            item.FailedPasswordWindowStartedAt = DateTime.MinValue;
            item.FailedPasswordWindowAttemptCount = 0;
            this._accounts.Store(item, @event.EventSourceId);
        }

        public void Handle(IPublishedEvent<AccountRoleAdded> @event)
        {
            AccountDocument item = this._accounts.GetById(@event.EventSourceId);

            item.SimpleRoles.Add(@event.Payload.Role);
            this._accounts.Store(item, @event.EventSourceId);
        }

        public void Handle(IPublishedEvent<AccountRoleRemoved> @event)
        {
            AccountDocument item = this._accounts.GetById(@event.EventSourceId);

            item.SimpleRoles.Remove(@event.Payload.Role);
            this._accounts.Store(item, @event.EventSourceId);
        }

        public void Handle(IPublishedEvent<AccountLoginFailed> @event)
        {
            AccountDocument item = this._accounts.GetById(@event.EventSourceId);

            item.FailedPasswordWindowStartedAt = @event.Payload.FailedPasswordWindowStartedAt;
            item.FailedPasswordWindowAttemptCount += 1;
            this._accounts.Store(item, @event.EventSourceId);
        }

        public void Handle(IPublishedEvent<AccountPasswordResetTokenChanged> @event)
        {
            AccountDocument item = this._accounts.GetById(@event.EventSourceId);

            item.PasswordResetToken = @event.Payload.PasswordResetToken;
            item.PasswordResetExpirationDate = @event.Payload.PasswordResetExpirationDate;
            this._accounts.Store(item, @event.EventSourceId);
        }

        private static string GetNormalizedUserName(string userName)
        {
            return userName.ToLower();
        }

        public void Handle(IPublishedEvent<AccountValidated> evnt) // Here added just to have this event registered in NCQRS
        {
        }
    }
}