using System;
using Machine.Specifications;
using Moq;
using Ncqrs.Commanding.ServiceModel;
using WB.Core.BoundedContexts.Designer.Views.Account;
using WB.UI.Shared.Web.MembershipProvider.Accounts;
using It = Machine.Specifications.It;

namespace WB.Core.BoundedContexts.Designer.Tests.AccountRepositoryTests
{

    internal class Create_should_return_membership_account__with_id_the_same_as_input_account_id : AccountRepositoryTestsContext
    {
        Establish context = () =>
        {
            validatedUserId = Guid.Parse("11111111111111111111111111111111");
            commandService = new Mock<ICommandService>();
            accountRepository = CreateAccountRepository(commandService: commandService.Object);
        };

        Because of = () =>
            validatedAccount = accountRepository.Create(validatedUserId, null, string.Empty, string.Empty);

        It should_validated_account_provider_user_key_not_be_null = () =>
            validatedAccount.ProviderUserKey.ShouldNotBeNull();

        It should_validated_acount_provider_user_key_be_type_of_guid = () =>
            validatedAccount.ProviderUserKey.ShouldBeOfType<Guid>();

        It should_validated_acount_provider_user_key_be_the_same_as_validated_user_id = () =>
            validatedAccount.ProviderUserKey.ShouldEqual(validatedUserId);

        private static CQRSAccountRepository accountRepository;
        private static Mock<ICommandService> commandService;
        private static Guid validatedUserId;
        private static IMembershipAccount validatedAccount;
    }
}
