using Moq;
using WB.Core.BoundedContexts.Designer.Aggregates;
using WB.Core.BoundedContexts.Designer.Views.Account;
using WB.Core.Infrastructure.PlainStorage;
using WB.Core.Infrastructure.ReadSide.Repository.Accessors;

namespace WB.Tests.Unit.Designer.BoundedContexts.Designer.AccountViewFactoryTests
{
    internal class AccountViewFactoryTestsContext
    {
        protected static User CreateAccount(string userName)
        {
            return new User { UserName = userName };
        }

        protected static AccountViewInputModel CreateAccountViewInputModel(string accountName)
        {
            return new AccountViewInputModel(accountName, null, null, null);
        }

        protected static AccountViewFactory CreateAccountViewFactory(IPlainStorageAccessor<User> accountsRepository = null)
        {
            return new AccountViewFactory(accountsRepository ?? Mock.Of<IPlainStorageAccessor<User>>());
        }
    }
}