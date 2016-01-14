﻿using System;
using Main.Core.Documents;
using Moq;
using Ncqrs.Eventing.ServiceModel.Bus;
using WB.Core.BoundedContexts.Designer.Views.Account;
using WB.Core.Infrastructure.EventBus;
using WB.Core.Infrastructure.EventBus.Lite;
using WB.Core.Infrastructure.ReadSide.Repository.Accessors;
using WB.UI.Designer.Providers.CQRS.Accounts;
using WB.UI.Designer.Providers.CQRS.Accounts.Events;

namespace WB.Tests.Unit.BoundedContexts.Designer.AccountDenormalizerTests
{
    internal class AccountDenormalizerTestsContext
    {
        protected static AccountDenormalizer CreateAccountDenormalizer(IReadSideRepositoryWriter<AccountDocument> accounts)
        {
            return new AccountDenormalizer(accounts ?? Mock.Of<IReadSideRepositoryWriter<AccountDocument>>());
        }

        protected static AccountDocument CreateAccountDocument(Guid userId)
        {
            return  new AccountDocument
            {
                ProviderUserKey = userId
            };
        }

        protected static IPublishedEvent<T> ToPublishedEvent<T>(T @event, Guid eventSourceId)
            where T : class, IEvent
        {
            return Mock.Of<IPublishedEvent<T>>(publishedEvent
                => publishedEvent.Payload == @event
                && publishedEvent.EventSourceId == eventSourceId);
        }

        protected static IPublishedEvent<AccountUpdated> CreateAccountUpdatedEvent(Guid userId, string userName)
        {
            return ToPublishedEvent(new AccountUpdated
            {
                UserName = userName
            }, eventSourceId: userId);
        }

        protected static IPublishedEvent<AccountRegistered> CreateAccountRegisteredEvent(Guid userId, string userName)
        {
            return ToPublishedEvent(new AccountRegistered
            {
                UserName = userName
            }, eventSourceId: userId);
        }
    }
}
