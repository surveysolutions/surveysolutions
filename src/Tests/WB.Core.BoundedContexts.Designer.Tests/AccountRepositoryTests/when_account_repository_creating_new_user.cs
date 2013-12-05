using System;
using Machine.Specifications;
using Moq;
using Ncqrs.Commanding.ServiceModel;
using WB.Core.BoundedContexts.Designer.Views.Account;
using WB.UI.Shared.Web.MembershipProvider.Accounts;
using It = Machine.Specifications.It;

namespace WB.Core.BoundedContexts.Designer.Tests.AccountRepositoryTests
{

    internal class when_account_repository_creating_new_user: AccountRepositoryTestsContext
    {
        Establish context = () =>
        {
            validatedUserId = Guid.Parse("11111111111111111111111111111111");
            commandService = new Mock<ICommandService>();
            accountRepository = CreateAccountRepository(commandService: commandService.Object);
        };

        Because of = () =>
            validatedAccount = accountRepository.Create(validatedUserId, null, string.Empty, string.Empty);

        It should_set_returned_account_provider_user_key_to_user_id = () =>
            validatedAccount.ProviderUserKey.ShouldEqual(validatedUserId);

        private static CQRSAccountRepository accountRepository;
        private static Mock<ICommandService> commandService;
        private static Guid validatedUserId;
        private static IMembershipAccount validatedAccount;
    }
}
