using System;
using Machine.Specifications;
using WB.Core.Infrastructure.CommandBus;
using WB.Core.Infrastructure.Implementation.CommandBus;

namespace WB.Tests.Integration.CommandServiceTests
{
    internal class when_executing_not_registered_command : CommandServiceTestsContext
    {
        private class NotRegisteredCommand : ICommand { public Guid CommandIdentifier { get; private set; } }

        Establish context = () =>
        {
            commandService = Create.CommandService();
        };

        Because of = () =>
            exception = Catch.Only<CommandServiceException>(() =>
                commandService.Execute(new NotRegisteredCommand(), null));

        It should_throw_exception_with_message_containing__not____registered__ = () =>
            exception.Message.ToLower().ToSeparateWords().ShouldContain("not", "registered");

        It should_throw_exception_with_message_containing_command_name = () =>
            exception.Message.ShouldContain(typeof(NotRegisteredCommand).Name);

        private static CommandServiceException exception;
        private static CommandService commandService;
    }
}