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
    public class when_handling_AccountUpdated_event_and_user_name_equals__ADMIN__ : AccountDenormalizerTestsContext
    {
        Establish context = () =>
        {
            accountStorage = new Mock<IReadSideRepositoryWriter<AccountDocument>>();

            AccountDocument account = CreateAccountDocument(userId);

            accountStorage.Setup(x => x.GetById(userId)).Returns(account);

            accountUpdatedEvent = CreateAccountUpdatedEvent(userId: userId, userName: "ADMIN");

            denormalizer = CreateAccountDenormalizer(accounts: accountStorage.Object);
        };

        Because of = () =>
            denormalizer.Handle(accountUpdatedEvent);

        It should_pass__admin__user_name_to_document_storages_Store_method = () =>
            accountStorage.Verify(s => s.Store(
                Moq.It.Is<AccountDocument>(d => d.UserName == "admin"),
                Moq.It.Is<Guid>(g => g == userId)));

        private static Guid userId = Guid.Parse("aaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaa");
        private static AccountDenormalizer denormalizer;
        private static IPublishedEvent<AccountUpdated> accountUpdatedEvent;
        private static Mock<IReadSideRepositoryWriter<AccountDocument>> accountStorage;
    }
}