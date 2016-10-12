using System;
using Machine.Specifications;
using Moq;
using Ncqrs.Eventing.ServiceModel.Bus;
using WB.Core.BoundedContexts.Designer.Aggregates;
using WB.Core.BoundedContexts.Designer.Views.Account;
using WB.Core.GenericSubdomains.Portable;
using WB.Core.Infrastructure.PlainStorage;
using WB.Core.Infrastructure.ReadSide.Repository.Accessors;
using WB.UI.Designer.Providers.CQRS.Accounts.Events;
using It = Machine.Specifications.It;

namespace WB.Tests.Unit.Designer.BoundedContexts.Designer.AccountDenormalizerTests
{
    //[Ignore("KP-7922 KP-7923")]
    //internal class when_handling_AccountUpdated_event_and_user_name_equals__ADMIN__ : AccountDenormalizerTestsContext
    //{
    //    Establish context = () =>
    //    {
    //        accountStorageMock = new Mock<IPlainStorageAccessor<Account>>();

    //        AccountDocument account = CreateAccountDocument(userId);

    //        accountStorageMock.Setup(x => x.GetById(userId.FormatGuid())).Returns(account);

    //        accountUpdatedEvent = CreateAccountUpdatedEvent(userId: userId, userName: "ADMIN");

    //        denormalizer = CreateAccountDenormalizer(accounts: accountStorageMock.Object);
    //    };

    //    Because of = () =>
    //        denormalizer.Handle(accountUpdatedEvent);

    //    It should_pass__admin__user_name_to_document_storages_Store_method = () =>
    //        accountStorageMock.Verify(s => s.Store(
    //            Moq.It.Is<AccountDocument>(d => d.UserName == "admin"),
    //            Moq.It.Is<string>(g => g == userId.FormatGuid())));

    //    private static Guid userId = Guid.Parse("aaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaa");
    //    private static AccountDenormalizer denormalizer;
    //    private static IPublishedEvent<AccountUpdated> accountUpdatedEvent;
    //    private static Mock<IPlainStorageAccessor<Account>> accountStorageMock;
    //}
}