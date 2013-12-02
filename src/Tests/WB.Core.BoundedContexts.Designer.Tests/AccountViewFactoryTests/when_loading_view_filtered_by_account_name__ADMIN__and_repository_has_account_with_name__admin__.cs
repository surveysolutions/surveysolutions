using System;
using System.Linq;
using Machine.Specifications;
using Moq;
using WB.Core.BoundedContexts.Designer.Views.Account;
using WB.Core.Infrastructure.ReadSide.Repository.Accessors;
using It = Machine.Specifications.It;
using it = Moq.It;

namespace WB.Core.BoundedContexts.Designer.Tests.AccountViewFactoryTests
{
    internal class when_loading_view_filtered_by_account_name__ADMIN__and_repository_has_account_with_name__admin__ : AccountViewFactoryTestsContext
    {
        Establish context = () =>
        {
            inputWithFilterByAccountName = CreateAccountViewInputModel(accountName: "ADMIN");

            var accountsRepositoryMock = new Mock<IQueryableReadSideRepositoryReader<AccountDocument>>();

            var repositoryDocuments = new[] { CreateAccountDocument(userName: "admin") };

            accountsRepositoryMock
                .Setup(x => x.Query<AccountView>(it.IsAny<Func<IQueryable<AccountDocument>, AccountView>>()))
                .Returns<Func<IQueryable<AccountDocument>, AccountView>>(func => func.Invoke(repositoryDocuments.AsQueryable()));

            accountFactory = CreateAccountViewFactory(accountsRepositoryMock: accountsRepositoryMock.Object);
        };

        Because of = () =>
            filteredAccount = accountFactory.Load(inputWithFilterByAccountName);

        It should_find_one_account = () =>
            filteredAccount.ShouldNotBeNull();

        private static AccountView filteredAccount;
        private static AccountViewInputModel inputWithFilterByAccountName;
        private static AccountViewFactory accountFactory;
    }
}
