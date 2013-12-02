using Moq;
using WB.Core.BoundedContexts.Designer.Views.Account;
using WB.Core.Infrastructure.ReadSide.Repository.Accessors;

namespace WB.Core.BoundedContexts.Designer.Tests.AccountViewFactoryTests
{
    internal class AccountViewFactoryTestsContext
    {
        protected static AccountDocument CreateAccountDocument(string userName)
        {
            return new AccountDocument { UserName = userName };
        }

        protected static AccountViewInputModel CreateAccountViewInputModel(string accountName)
        {
            return new AccountViewInputModel(accountName, null, null, null);
        }

        protected static AccountViewFactory CreateAccountViewFactory(IQueryableReadSideRepositoryReader<AccountDocument> accountsRepositoryMock = null)
        {
            return new AccountViewFactory(accountsRepositoryMock ?? Mock.Of<IQueryableReadSideRepositoryReader<AccountDocument>>());
        }
    }
}