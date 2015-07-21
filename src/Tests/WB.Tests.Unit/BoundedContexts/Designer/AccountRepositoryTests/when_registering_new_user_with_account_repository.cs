using System;
using Machine.Specifications;
using Moq;
using WB.Core.BoundedContexts.Designer.Commands.Account;
using WB.Core.BoundedContexts.Designer.Views.Account;
using WB.Core.Infrastructure.CommandBus;
using It = Machine.Specifications.It;

namespace WB.Tests.Unit.BoundedContexts.Designer.AccountRepositoryTests
{

    internal class when_registering_new_user_with_account_repository : AccountRepositoryTestsContext
    {
        Establish context = () =>
        {
            validatedUserId = Guid.Parse("11111111111111111111111111111111");
            commandService = new Mock<ICommandService>();
            accountRepository = CreateAccountRepository(commandService: commandService.Object);
        };

        Because of = () =>
            accountRepository.Register(new AccountView() { ProviderUserKey = validatedUserId });

        It should_execute_RegisterAccountCommand_with_specified_validatedUserId = () =>
            commandService.Verify(command => command.Execute(Moq.It.Is<RegisterAccountCommand>(cp => cp.AccountId == validatedUserId), Moq.It.IsAny<string>(), Moq.It.IsAny<bool>()));

        private static CQRSAccountRepository accountRepository;
        private static Mock<ICommandService> commandService;
        private static Guid validatedUserId;
    }
}
