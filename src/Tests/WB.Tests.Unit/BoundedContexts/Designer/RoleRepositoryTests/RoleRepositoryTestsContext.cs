using Machine.Specifications;
using Moq;
using WB.Core.BoundedContexts.Designer.Views.Account;
using WB.Core.Infrastructure.CommandBus;
using WB.Core.Infrastructure.ReadSide;

namespace WB.Tests.Unit.BoundedContexts.Designer.RoleRepositoryTests
{
    [Subject(typeof(CQRSRoleRepository))]
    internal class RoleRepositoryTestsContext
    {
        protected static CQRSRoleRepository CreateRoleRepository(ICommandService commandService = null,
            IViewFactory<AccountListViewInputModel, AccountListView> accountListViewFactory = null,
            IViewFactory<AccountViewInputModel, AccountView> accountViewFactory = null)
        {
            return new CQRSRoleRepository(commandService: commandService ?? Mock.Of<ICommandService>(),
                accountListViewFactory: accountListViewFactory ?? Mock.Of<IViewFactory<AccountListViewInputModel, AccountListView>>(),
                accountViewFactory: accountViewFactory ?? Mock.Of<IViewFactory<AccountViewInputModel, AccountView>>());
        }
    }
}
