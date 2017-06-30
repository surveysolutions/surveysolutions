using System;
using Machine.Specifications;
using Moq;
using WB.Core.GenericSubdomains.Portable.ServiceLocation;
using WB.Core.Infrastructure.Aggregates;
using WB.Core.Infrastructure.CommandBus;
using WB.Core.Infrastructure.CommandBus.Implementation;
using It = Machine.Specifications.It;

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

        Establish context = () =>
        {
            CommandRegistry
                .Setup<Aggregate>()
                .InitializesWith<PlainConstructingCommand>(_ => Guid.Empty, aggregate => aggregate.Handle);

            var serviceLocator = Mock.Of<IServiceLocator>(_
                => _.GetInstance(typeof(Aggregate)) == new Aggregate());

            commandService = Abc.Create.Service.CommandService(serviceLocator: serviceLocator);
        };

        Because of = () =>
            commandService.Execute(executedCommand, null);

        It should_pass_command_to_be_handled_by_plain_aggregate_root = () =>
            handledCommand.ShouldEqual(executedCommand);

        private static CommandService commandService;
        private static PlainConstructingCommand handledCommand = null;
        private static PlainConstructingCommand executedCommand = new PlainConstructingCommand();
    }
}