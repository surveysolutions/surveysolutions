using Machine.Specifications;
using Moq;
using WB.Core.BoundedContexts.Designer.Views.Account;
using WB.Core.Infrastructure.CommandBus;
using WB.Core.Infrastructure.ReadSide;

namespace WB.Tests.Unit.Designer.BoundedContexts.Designer.AccountRepositoryTests
{
    [Subject(typeof(DesignerAccountRepository))]
    internal class AccountRepositoryTestsContext
    {
        protected static DesignerAccountRepository CreateAccountRepository(ICommandService commandService = null,
            IAccountListViewFactory accountListViewFactory = null,
            IAccountViewFactory accountViewFactory = null)
        {
            return new DesignerAccountRepository(commandService: commandService ?? Mock.Of<ICommandService>(),
                accountListViewFactory: accountListViewFactory ?? Mock.Of<IAccountListViewFactory>(),
                accountViewFactory: accountViewFactory ?? Mock.Of<IAccountViewFactory>());
        }
    }
}
