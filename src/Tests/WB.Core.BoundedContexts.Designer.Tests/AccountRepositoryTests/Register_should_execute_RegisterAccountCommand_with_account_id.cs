using System;
using Machine.Specifications;
using Moq;
using Ncqrs.Commanding.ServiceModel;
using WB.Core.BoundedContexts.Designer.Commands.Account;
using WB.Core.BoundedContexts.Designer.Views.Account;
using It = Machine.Specifications.It;

namespace WB.Core.BoundedContexts.Designer.Tests.AccountRepositoryTests
{

    internal class Register_should_execute_RegisterAccountCommand_with_account_id_the_same_as_input_membership_account_id : AccountRepositoryTestsContext
    {
        Establish context = () =>
        {
            validatedUserId = Guid.Parse("11111111111111111111111111111111");
            commandService = new Mock<ICommandService>();
            accountRepository = CreateAccountRepository(commandService: commandService.Object);
        };

        Because of = () =>
            accountRepository.Register(new AccountView() { ProviderUserKey = validatedUserId });

        It should_execute_RegisterAccountCommand_with_account_id_the_same_as_validatedUserId = () =>
            commandService.Verify(command => command.Execute(Moq.It.Is<RegisterAccountCommand>(cp => cp.AccountId == validatedUserId)));

        private static CQRSAccountRepository accountRepository;
        private static Mock<ICommandService> commandService;
        private static Guid validatedUserId;
    }
}
