using System;
using Machine.Specifications;
using Moq;
using WB.Core.BoundedContexts.Designer.Aggregates;
using WB.Core.BoundedContexts.Designer.Commands.Account;
using WB.Core.BoundedContexts.Designer.Views.Account;
using WB.Core.Infrastructure.CommandBus;
using It = Machine.Specifications.It;

namespace WB.Tests.Unit.Designer.BoundedContexts.Designer.AccountRepositoryTests
{

    internal class when_registering_new_user_with_account_repository : AccountRepositoryTestsContext
    {
        [NUnit.Framework.OneTimeSetUp] public void context () {
            validatedUserId = Guid.Parse("11111111111111111111111111111111");
            commandService = new Mock<ICommandService>();
            accountRepository = CreateAccountRepository(commandService: commandService.Object);
        }

        private void BecauseOf() =>
            accountRepository.Register(new User { ProviderUserKey = validatedUserId });

        [NUnit.Framework.Test] public void should_execute_RegisterAccountCommand_with_specified_validatedUserId () =>
            commandService.Verify(command => command.Execute(Moq.It.Is<RegisterUser>(cp => cp.UserId == validatedUserId), Moq.It.IsAny<string>()));

        private static DesignerAccountRepository accountRepository;
        private static Mock<ICommandService> commandService;
        private static Guid validatedUserId;
    }
}
