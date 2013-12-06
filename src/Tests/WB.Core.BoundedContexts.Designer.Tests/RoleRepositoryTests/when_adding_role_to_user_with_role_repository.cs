using System;
using Machine.Specifications;
using Moq;
using Ncqrs.Commanding.ServiceModel;
using WB.Core.BoundedContexts.Designer.Commands.Account;
using WB.Core.BoundedContexts.Designer.Views.Account;
using WB.UI.Shared.Web.MembershipProvider.Roles;
using It = Machine.Specifications.It;

namespace WB.Core.BoundedContexts.Designer.Tests.RoleRepositoryTests
{
    internal class when_adding_role_to_user_with_role_repository : RoleRepositoryTestsContext
    {
        Establish context = () =>
        {
            validatedUserId = Guid.Parse("11111111111111111111111111111111");
            validatedRole = SimpleRoleEnum.User;
            commandService = new Mock<ICommandService>();
            roleRepository = CreateRoleRepository(commandService: commandService.Object);
        };

        Because of = () =>
            roleRepository.AddUserToRole(null, validatedRole.ToString(), validatedUserId);

        It should_execute_AddRoleToAccountCommand_with_specified_UserId = () =>
            commandService.Verify(command => command.Execute(Moq.It.Is<AddRoleToAccountCommand>(cp => cp.AccountId == validatedUserId)));

        It should_execute_AddRoleToAccountCommand_with_specified_role = () =>
            commandService.Verify(command => command.Execute(Moq.It.Is<AddRoleToAccountCommand>(cp => cp.Role == validatedRole)));

        private static CQRSRoleRepository roleRepository;
        private static Mock<ICommandService> commandService;
        private static Guid validatedUserId;
        private static SimpleRoleEnum validatedRole;
    }
}
