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
                                       IEventHandler<AccountPasswordResetTokenChanged>
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

        public void Handle(IPublishedEvent<AccountRegistered> @event)
        {
            this._accounts.Store(null,
                @event.EventSourceId);
        }

        public void Handle(IPublishedEvent<AccountConfirmed> @event)
        {
            AccountDocument item = this._accounts.GetById(@event.EventSourceId);
            this._accounts.Store(item, @event.EventSourceId);
        }

        public void Handle(IPublishedEvent<AccountDeleted> @event)
        {
            this._accounts.Remove(@event.EventSourceId);
        }

        public void Handle(IPublishedEvent<AccountLocked> @event)
        {
            AccountDocument item = this._accounts.GetById(@event.EventSourceId);

            this._accounts.Store(item, @event.EventSourceId);
        }

        public void Handle(IPublishedEvent<AccountOnlineUpdated> @event)
        {
            AccountDocument item = this._accounts.GetById(@event.EventSourceId);

            this._accounts.Store(item, @event.EventSourceId);
        }

        public void Handle(IPublishedEvent<AccountPasswordChanged> @event)
        {
            AccountDocument item = this._accounts.GetById(@event.EventSourceId);

            this._accounts.Store(item, @event.EventSourceId);
        }

        public void Handle(IPublishedEvent<AccountPasswordQuestionAndAnswerChanged> @event)
        {
            AccountDocument item = this._accounts.GetById(@event.EventSourceId);

            this._accounts.Store(item, @event.EventSourceId);
        }

        public void Handle(IPublishedEvent<AccountPasswordReset> @event)
        {
            AccountDocument item = this._accounts.GetById(@event.EventSourceId);

            this._accounts.Store(item, @event.EventSourceId);
        }

        public void Handle(IPublishedEvent<AccountUnlocked> @event)
        {
            AccountDocument item = this._accounts.GetById(@event.EventSourceId);

            this._accounts.Store(item, @event.EventSourceId);
        }

        public void Handle(IPublishedEvent<AccountUpdated> @event)
        {
            AccountDocument item = this._accounts.GetById(@event.EventSourceId);

            this._accounts.Store(item, @event.EventSourceId);
        }

        public void Handle(IPublishedEvent<UserLoggedIn> @event)
        {
            AccountDocument item = this._accounts.GetById(@event.EventSourceId);

            this._accounts.Store(item, @event.EventSourceId);
        }

        public void Handle(IPublishedEvent<AccountRoleAdded> @event)
        {
            AccountDocument item = this._accounts.GetById(@event.EventSourceId);

            this._accounts.Store(item, @event.EventSourceId);
        }

        public void Handle(IPublishedEvent<AccountRoleRemoved> @event)
        {
            AccountDocument item = this._accounts.GetById(@event.EventSourceId);

            this._accounts.Store(item, @event.EventSourceId);
        }

        public void Handle(IPublishedEvent<AccountLoginFailed> @event)
        {
            AccountDocument item = this._accounts.GetById(@event.EventSourceId);

            this._accounts.Store(item, @event.EventSourceId);
        }

        public void Handle(IPublishedEvent<AccountPasswordResetTokenChanged> @event)
        {
            AccountDocument item = this._accounts.GetById(@event.EventSourceId);

            this._accounts.Store(item, @event.EventSourceId);
        }
    }
}