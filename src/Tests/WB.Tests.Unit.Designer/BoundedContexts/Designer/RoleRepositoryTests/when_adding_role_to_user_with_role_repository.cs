using System;
using Machine.Specifications;
using Moq;
using WB.Core.BoundedContexts.Designer.Commands.Account;
using WB.Core.BoundedContexts.Designer.MembershipProvider.Roles;
using WB.Core.BoundedContexts.Designer.Views.Account;
using WB.Core.Infrastructure.CommandBus;


namespace WB.Tests.Unit.Designer.BoundedContexts.Designer.RoleRepositoryTests
{
    internal class when_adding_role_to_user_with_role_repository : RoleRepositoryTestsContext
    {
        [NUnit.Framework.OneTimeSetUp] public void context () {
            validatedUserId = Guid.Parse("11111111111111111111111111111111");
            validatedRole = SimpleRoleEnum.User;
            commandService = new Mock<ICommandService>();
            roleRepository = CreateRoleRepository(commandService: commandService.Object);
            BecauseOf();
        }

        private void BecauseOf() =>
            roleRepository.AddUserToRole(null, validatedRole.ToString(), validatedUserId);

        [NUnit.Framework.Test] public void should_execute_AddRoleToAccountCommand_with_specified_UserId () =>
            commandService.Verify(command => command.Execute(Moq.It.Is<AssignUserRole>(cp => cp.UserId == validatedUserId), Moq.It.IsAny<string>()));

        [NUnit.Framework.Test] public void should_execute_AddRoleToAccountCommand_with_specified_role () =>
            commandService.Verify(command => command.Execute(Moq.It.Is<AssignUserRole>(cp => cp.Role == validatedRole), Moq.It.IsAny<string>()));

        private static CQRSRoleRepository roleRepository;
        private static Mock<ICommandService> commandService;
        private static Guid validatedUserId;
        private static SimpleRoleEnum validatedRole;
    }
}
