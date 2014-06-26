using Machine.Specifications;
using Main.Core.Entities.SubEntities;
using Main.Core.Utility;
using Moq;
using Ncqrs.Commanding;
using Ncqrs.Commanding.ServiceModel;
using WB.Core.SharedKernels.DataCollection.Commands.User;
using WB.Core.SharedKernels.SurveyManagement.Web.Models;
using WB.UI.Headquarters.Controllers;
using WB.UI.Headquarters.Tests.InstallControllerTests;
using It = Machine.Specifications.It;

namespace WB.UI.Headquarters.Tests.InterviewControllerTests
{
    internal class when_finish_installation : InstallControllerTestsContext
    {
        private Establish context = () =>
        {
            commandServiceMock.Setup(_ => _.Execute(Moq.It.IsAny<ICommand>(), Moq.It.IsAny<string>()))
                .Callback<ICommand, string>((command, origin) => executedCommand = command);
            controller = CreateController(commandService:commandServiceMock.Object);
        };

        Because of = () =>
            controller.Finish(model);

        It should_execute_command_service_only_once = () =>
            commandServiceMock.Verify(_=>_.Execute(executedCommand, null), Times.Once);

        It should_execute_command_be_type_of_CreateUserCommand = () =>
            executedCommand.ShouldBeOfExactType<CreateUserCommand>();

        It should_execute_command_user_name_be_equal_model_user_name = () =>
            GetSpecifiedCommand().UserName.ShouldEqual(model.UserName);

        It should_execute_command_email_be_equal_model_email = () =>
            GetSpecifiedCommand().Email.ShouldEqual(model.Email);

        It should_execute_command_IsLockedByHQ_be_equal_to_false = () =>
            GetSpecifiedCommand().IsLockedByHQ.ShouldBeFalse();

        It should_execute_command_IsLockedBySupervisor_be_equal_to_false = () =>
            GetSpecifiedCommand().IsLockedBySupervisor.ShouldBeFalse();

        It should_execute_command_Roles_contains_only_hq_role = () =>
            GetSpecifiedCommand().Roles.ShouldContainOnly(UserRoles.Headquarter);

        It should_execute_command_Supervisor_be_null = () =>
            GetSpecifiedCommand().Supervisor.ShouldBeNull();

        It should_execute_command_Password_be_computed_hash_by_model_password = () =>
            GetSpecifiedCommand().Password.ShouldEqual(SimpleHash.ComputeHash(model.Password));

        private static CreateUserCommand GetSpecifiedCommand()
        {
            return executedCommand as CreateUserCommand;
        }

        private static Mock<ICommandService> commandServiceMock = new Mock<ICommandService>();
        private static ICommand executedCommand;
        private static InstallController controller;

        private static UserModel model = new UserModel()
        {
            UserName = "hq",
            Password = "Qwerty1234",
            ConfirmPassword = "Qwerty1234",
            Email = "hq@wbcapi.org"
        };
    }
}
