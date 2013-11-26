using System;
using Main.Core.Documents;
using Moq;
using Ncqrs.Eventing.ServiceModel.Bus;
using WB.Core.BoundedContexts.Designer.Views.Account;
using WB.Core.Infrastructure.ReadSide.Repository.Accessors;
using WB.UI.Designer.Providers.CQRS.Accounts;
using WB.UI.Designer.Providers.CQRS.Accounts.Events;

namespace WB.Core.BoundedContexts.Designer.Tests.AccountDenormalizerTests
{
    public class AccountDenormalizerTestsContext
    {
        public static AccountDenormalizer CreateAccountDenormalizer(IReadSideRepositoryWriter<AccountDocument> accounts)
        {
            return new AccountDenormalizer(accounts ?? Mock.Of<IReadSideRepositoryWriter<AccountDocument>>());
        }

        public static AccountDocument CreateAccountDocument(Guid userId)
        {
            return  new AccountDocument
            {
                ProviderUserKey = userId
            };
        }

        protected static IPublishedEvent<T> ToPublishedEvent<T>(T @event, Guid eventSourceId)
            where T : class
        {
            return Mock.Of<IPublishedEvent<T>>(publishedEvent
                => publishedEvent.Payload == @event
                && publishedEvent.EventSourceId == eventSourceId);
        }

        public static IPublishedEvent<AccountUpdated> CreateAccountUpdatedEvent(Guid userId, string userName)
        {
            return ToPublishedEvent(new AccountUpdated
            {
                UserName = userName
            }, eventSourceId: userId);
        }

        public static IPublishedEvent<AccountRegistered> CreateCreateAccountRegisteredEventEvent(Guid userId, string userName)
        {
            return ToPublishedEvent(new AccountRegistered
            {
                ProviderUserKey = userId,
                UserName = userName
            }, eventSourceId: userId);
        }
    }
}
