using System;
using Machine.Specifications;
using Moq;
using Ncqrs.Eventing.ServiceModel.Bus;
using WB.Core.BoundedContexts.Designer.Views.Account;
using WB.Core.GenericSubdomains.Portable;
using WB.Core.Infrastructure.ReadSide.Repository.Accessors;
using WB.UI.Designer.Providers.CQRS.Accounts;
using WB.UI.Designer.Providers.CQRS.Accounts.Events;
using It = Machine.Specifications.It;

namespace WB.Tests.Unit.BoundedContexts.Designer.AccountDenormalizerTests
{
    internal class when_handling_AccountRegistered_event_and_user_name_equals__ADMIN__ : AccountDenormalizerTestsContext
    {
        Establish context = () =>
        {
            accountStorageMock = new Mock<IReadSideRepositoryWriter<AccountDocument>>();

            accountRegisteredEvent = CreateAccountRegisteredEvent(userId: userId, userName: "ADMIN");

            denormalizer = CreateAccountDenormalizer(accounts: accountStorageMock.Object);
        };

        Because of = () =>
            denormalizer.Handle(accountRegisteredEvent);

        It should_pass__admin__user_name_to_document_storages_Store_method = () =>
            accountStorageMock.Verify(s => s.Store(
                Moq.It.Is<AccountDocument>(d => d.UserName == "admin"),
                Moq.It.Is<string>(g => g == userId.FormatGuid())));

        private static Guid userId = Guid.Parse("aaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaa");
        private static AccountDenormalizer denormalizer;
        private static IPublishedEvent<AccountRegistered> accountRegisteredEvent;
        private static Mock<IReadSideRepositoryWriter<AccountDocument>> accountStorageMock;
    }
}