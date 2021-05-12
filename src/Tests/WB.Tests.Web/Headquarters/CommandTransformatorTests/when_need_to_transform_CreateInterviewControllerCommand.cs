using FluentAssertions;
using WB.Core.Infrastructure.CommandBus;
using WB.Core.SharedKernels.DataCollection.Commands.Interview;
using WB.UI.Headquarters.Services.Impl;

namespace WB.Tests.Web.Headquarters.CommandTransformatorTests
{
    internal class when_need_to_transform_CreateInterviewControllerCommand: CommandTransformatorTestsContext
    {
        [NUnit.Framework.OneTimeSetUp] public void context () {
            commandTransformator = CreateCommandTransformator();
            BecauseOf();
        }

        public void BecauseOf() =>
            command = commandTransformator.TransformCommandIfNeeded(command);

        [NUnit.Framework.Test] public void should_return_command_of_CreateInterviewCommand_type () =>
            command.Should().BeOfType<CreateInterview>();

        private static CommandTransformator commandTransformator;
        private static ICommand command = Create.Command.CreateInterviewControllerCommand();
    }
}
