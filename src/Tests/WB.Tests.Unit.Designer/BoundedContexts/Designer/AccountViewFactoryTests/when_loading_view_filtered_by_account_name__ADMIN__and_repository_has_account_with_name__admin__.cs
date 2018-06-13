using FluentAssertions;
using WB.Core.BoundedContexts.Designer.Aggregates;
using WB.Core.BoundedContexts.Designer.Views.Account;
using WB.Core.Infrastructure.PlainStorage;
using it = Moq.It;

namespace WB.Tests.Unit.Designer.BoundedContexts.Designer.AccountViewFactoryTests
{
    internal class when_loading_view_filtered_by_account_name__ADMIN__and_repository_has_account_with_name__admin__ : AccountViewFactoryTestsContext
    {
        [NUnit.Framework.OneTimeSetUp] public void context () {
            inputWithFilterByAccountName = CreateAccountViewInputModel(accountName: "ADMIN");

            var accountsRepository = Stub<IPlainStorageAccessor<User>>.Returning(CreateAccount(userName: "admin"));

            accountFactory = CreateAccountViewFactory(accountsRepository: accountsRepository);
            BecauseOf();
        }

        private void BecauseOf() =>
            filteredAccount = accountFactory.Load(inputWithFilterByAccountName);

        [NUnit.Framework.Test] public void should_find_one_account () =>
            filteredAccount.Should().NotBeNull(); 

        private static IAccountView filteredAccount;
        private static AccountViewInputModel inputWithFilterByAccountName;
        private static AccountViewFactory accountFactory;
    }
}
