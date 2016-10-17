using System;
using System.Linq;
using Machine.Specifications;
using Moq;
using WB.Core.BoundedContexts.Designer.Aggregates;
using WB.Core.BoundedContexts.Designer.Views.Account;
using WB.Core.Infrastructure.PlainStorage;
using WB.Core.Infrastructure.ReadSide.Repository.Accessors;
using It = Machine.Specifications.It;
using it = Moq.It;

namespace WB.Tests.Unit.Designer.BoundedContexts.Designer.AccountViewFactoryTests
{
    internal class when_loading_view_filtered_by_account_name__ADMIN__and_repository_has_account_with_name__admin__ : AccountViewFactoryTestsContext
    {
        Establish context = () =>
        {
            inputWithFilterByAccountName = CreateAccountViewInputModel(accountName: "ADMIN");

            var accountsRepositoryMock = new Mock<IPlainStorageAccessor<User>>();

            accountsRepositoryMock
                .Setup(x => x.Query<IAccountView>(it.IsAny<Func<IQueryable<User>, IAccountView>>()))
                .Returns(CreateAccount(userName: "admin"));

            accountFactory = CreateAccountViewFactory(accountsRepository: accountsRepositoryMock.Object);
        };

        Because of = () =>
            filteredAccount = accountFactory.Load(inputWithFilterByAccountName);

        It should_find_one_account = () =>
            filteredAccount.ShouldNotBeNull(); 

        private static IAccountView filteredAccount;
        private static AccountViewInputModel inputWithFilterByAccountName;
        private static AccountViewFactory accountFactory;
    }
}
