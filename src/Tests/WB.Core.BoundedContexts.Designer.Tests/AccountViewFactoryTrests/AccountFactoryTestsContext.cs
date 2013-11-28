using Moq;
using WB.Core.BoundedContexts.Designer.Views.Account;
using WB.Core.Infrastructure.ReadSide.Repository.Accessors;

namespace WB.Core.BoundedContexts.Designer.Tests.AccountViewFactoryTrests
{
    public class AccountFactoryTestsContext
    {
        public static AccountDocument CreateAccountDocument(string userName)
        {
            return new AccountDocument { UserName = userName };
        }

        public static AccountViewInputModel CreateAccountViewInputModel(string accountName)
        {
            return new AccountViewInputModel(accountName, null, null, null);
        }

        public static AccountViewFactory CreateAccountViewFactory(IQueryableReadSideRepositoryReader<AccountDocument> accountsRepositoryMock = null)
        {
            return new AccountViewFactory(accountsRepositoryMock ?? Mock.Of<IQueryableReadSideRepositoryReader<AccountDocument>>());
        }
    }
}