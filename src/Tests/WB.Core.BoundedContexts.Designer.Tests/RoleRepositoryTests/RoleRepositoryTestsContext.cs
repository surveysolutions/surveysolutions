using Machine.Specifications;
using Main.Core.View;
using Moq;
using Ncqrs.Commanding.ServiceModel;
using WB.Core.BoundedContexts.Designer.Views.Account;

namespace WB.Core.BoundedContexts.Designer.Tests.RoleRepositoryTests
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
