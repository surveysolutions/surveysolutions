using Machine.Specifications;
using Moq;
using WB.Core.BoundedContexts.Designer.Views.Account;
using WB.Core.Infrastructure.CommandBus;
using WB.Core.Infrastructure.ReadSide;

namespace WB.Tests.Unit.BoundedContexts.Designer.AccountRepositoryTests
{
    [Subject(typeof(CQRSAccountRepository))]
    internal class AccountRepositoryTestsContext
    {
        protected static CQRSAccountRepository CreateAccountRepository(ICommandService commandService = null,
            IViewFactory<AccountListViewInputModel, AccountListView> accountListViewFactory = null,
            IViewFactory<AccountViewInputModel, AccountView> accountViewFactory = null)
        {
            return new CQRSAccountRepository(commandService: commandService ?? Mock.Of<ICommandService>(),
                accountListViewFactory: accountListViewFactory ?? Mock.Of<IViewFactory<AccountListViewInputModel, AccountListView>>(),
                accountViewFactory: accountViewFactory ?? Mock.Of<IViewFactory<AccountViewInputModel, AccountView>>());
        }
    }
}
