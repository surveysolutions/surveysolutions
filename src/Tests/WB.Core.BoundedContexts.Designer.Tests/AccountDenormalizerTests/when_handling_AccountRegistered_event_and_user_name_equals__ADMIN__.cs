using System;
using Machine.Specifications;
using Moq;
using Ncqrs.Eventing.ServiceModel.Bus;
using WB.Core.BoundedContexts.Designer.Views.Account;
using WB.Core.Infrastructure.ReadSide.Repository.Accessors;
using WB.UI.Designer.Providers.CQRS.Accounts;
using WB.UI.Designer.Providers.CQRS.Accounts.Events;
using It = Machine.Specifications.It;

namespace WB.Core.BoundedContexts.Designer.Tests.AccountDenormalizerTests
{
    public class when_handling_AccountRegistered_event_and_user_name_equals__ADMIN__ : AccountDenormalizerTestsContext
    {
        Establish context = () =>
        {
            accountStorage = new Mock<IReadSideRepositoryWriter<AccountDocument>>();

            accountRegisteredEvent = CreateCreateAccountRegisteredEventEvent(userId: userId, userName: "ADMIN");

            denormalizer = CreateAccountDenormalizer(accounts: accountStorage.Object);
        };

        Because of = () =>
            denormalizer.Handle(accountRegisteredEvent);

        It should_pass__admin__user_name_to_document_storages_Store_method = () =>
            accountStorage.Verify(s => s.Store(
                Moq.It.Is<AccountDocument>(d => d.UserName == "admin"),
                Moq.It.Is<Guid>(g => g == userId)));

        private static Guid userId = Guid.Parse("aaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaa");
        private static AccountDenormalizer denormalizer;
        private static IPublishedEvent<AccountRegistered> accountRegisteredEvent;
        private static Mock<IReadSideRepositoryWriter<AccountDocument>> accountStorage;
    }
}