using System;
using FluentAssertions;
using Moq;
using WB.Core.GenericSubdomains.Portable.ServiceLocation;
using WB.Core.Infrastructure.Aggregates;
using WB.Core.Infrastructure.CommandBus;
using WB.Core.Infrastructure.CommandBus.Implementation;

namespace WB.Tests.Integration.CommandServiceTests
{
    internal class when_executing_constructing_command_handled_by_plain_aggregate_root
    {
        private class PlainConstructingCommand : ICommand { public Guid CommandIdentifier { get; private set; } }

        private class Aggregate : IPlainAggregateRoot
        {
            public void SetId(Guid id) {}
            public void Handle(PlainConstructingCommand command) => handledCommand = command;
        }

        [NUnit.Framework.OneTimeSetUp] public void context () {
            CommandRegistry
                .Setup<Aggregate>()
                .InitializesWith<PlainConstructingCommand>(_ => Guid.Empty, aggregate => aggregate.Handle);

            var serviceLocator = Mock.Of<IServiceLocator>(_
                => _.GetInstance(typeof(Aggregate)) == new Aggregate());

            commandService = Abc.Create.Service.CommandService(serviceLocator: serviceLocator);

            BecauseOf();
        }

        private void BecauseOf() =>
            commandService.Execute(executedCommand, null);

        [NUnit.Framework.Test] public void should_pass_command_to_be_handled_by_plain_aggregate_root () =>
            handledCommand.Should().Be(executedCommand);

        private static CommandService commandService;
        private static PlainConstructingCommand handledCommand = null;
        private static PlainConstructingCommand executedCommand = new PlainConstructingCommand();
    }
}
