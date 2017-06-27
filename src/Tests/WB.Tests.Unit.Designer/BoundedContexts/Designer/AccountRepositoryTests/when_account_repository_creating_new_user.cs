using System;
using Machine.Specifications;
using Moq;
using WB.Core.BoundedContexts.Designer.Services.Accounts;
using WB.Core.BoundedContexts.Designer.Views.Account;
using WB.Core.Infrastructure.CommandBus;


namespace WB.Tests.Unit.Designer.BoundedContexts.Designer.AccountRepositoryTests
{

    internal class when_account_repository_creating_new_user: AccountRepositoryTestsContext
    {
        [NUnit.Framework.OneTimeSetUp] public void context () {
            validatedUserId = Guid.Parse("11111111111111111111111111111111");
            commandService = new Mock<ICommandService>();
            accountRepository = CreateAccountRepository(commandService: commandService.Object);
            BecauseOf();
        }

        private void BecauseOf() =>
            validatedAccount = accountRepository.Create(validatedUserId, null, string.Empty, string.Empty);

        [NUnit.Framework.Test] public void should_set_returned_account_provider_user_key_to_user_id () =>
            validatedAccount.ProviderUserKey.ShouldEqual(validatedUserId);

        private static DesignerAccountRepository accountRepository;
        private static Mock<ICommandService> commandService;
        private static Guid validatedUserId;
        private static IMembershipAccount validatedAccount;
    }
}
