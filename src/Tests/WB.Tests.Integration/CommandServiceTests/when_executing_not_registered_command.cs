using System;
using System.Threading;
using FluentAssertions;
using Machine.Specifications;
using WB.Core.Infrastructure.CommandBus;
using WB.Core.Infrastructure.CommandBus.Implementation;
using It = Machine.Specifications.It;

namespace WB.Tests.Integration.CommandServiceTests
{
    internal class when_executing_not_registered_command
    {
        private class NotRegisteredCommand : ICommand { public Guid CommandIdentifier { get; private set; } }

        [NUnit.Framework.OneTimeSetUp] public void context () {
            commandService = Abc.Create.Service.CommandService();
            BecauseOf();
        }

        private void BecauseOf() =>
            exception = Catch.Only<CommandServiceException>(() =>
                commandService.Execute(new NotRegisteredCommand(), null));

        [NUnit.Framework.Test] public void should_throw_exception_with_message_containing__not____registered__ () =>
            exception.Message.ToLower().ToSeparateWords().Should().Contain("not", "registered");

        [NUnit.Framework.Test] public void should_throw_exception_with_message_containing_command_name () =>
            exception.Message.Should().Contain(typeof(NotRegisteredCommand).Name);

        private static CommandServiceException exception;
        private static CommandService commandService;
    }
}
