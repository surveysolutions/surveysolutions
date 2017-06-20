using Machine.Specifications;
using Ncqrs.Commanding;
using WB.Core.Infrastructure.CommandBus;
using WB.Core.SharedKernels.DataCollection.Commands.Interview;
using WB.Tests.Abc;
using WB.UI.Headquarters.Code.CommandTransformation;
using It = Machine.Specifications.It;

namespace WB.Tests.Unit.SharedKernels.SurveyManagement.Web.CommandTransformatorTests
{
    internal class when_need_to_transform_CreateInterviewControllerCommand: CommandTransformatorTestsContext
    {
        Establish context = () =>
        {
            commandTransformator = CreateCommandTransformator();
        };

        Because of = () =>
            command = commandTransformator.TransformCommnadIfNeeded(command);

        It should_return_command_of_CreateInterviewCommand_type = () =>
            command.ShouldBeOfExactType<CreateInterview>();

        private static CommandTransformator commandTransformator;
        private static ICommand command = Create.Command.CreateInterviewControllerCommand();
    }
}
