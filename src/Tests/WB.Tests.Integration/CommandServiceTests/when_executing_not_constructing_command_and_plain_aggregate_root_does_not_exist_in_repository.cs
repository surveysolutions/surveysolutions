using System;
using Machine.Specifications;
using Moq;
using Ncqrs.Domain;
using WB.Core.GenericSubdomains.Portable;
using WB.Core.Infrastructure.Aggregates;
using WB.Core.Infrastructure.CommandBus;
using WB.Core.Infrastructure.CommandBus.Implementation;
using It = Machine.Specifications.It;

namespace WB.Tests.Integration.CommandServiceTests
{
    internal class when_executing_not_constructing_command_and_plain_aggregate_root_does_not_exist_in_repository
    {
        private class NotConstructingPlainCommand : ICommand { public Guid CommandIdentifier { get; private set; } }
        private class Aggregate : IPlainAggregateRoot { public void SetId(Guid id) {} }

        Establish context = () =>
        {
            CommandRegistry
                .Setup<Aggregate>()
                .Handles<NotConstructingPlainCommand>(_ => aggregateId, (command, aggregate) => { });

            var repository = Mock.Of<IPlainAggregateRootRepository>(_
                => _.Get(typeof(Aggregate), aggregateId) == null as Aggregate);

            commandService = Abc.Create.Service.CommandService(plainRepository: repository);
        };

        Because of = () =>
            exception = Catch.Only<CommandServiceException>(() =>
                commandService.Execute(new NotConstructingPlainCommand(), null));

        It should_throw_exception_with_message_containing__unable____constructing__ = () =>
            exception.Message.ToLower().ToSeparateWords().ShouldContain("unable", "constructing");

        It should_throw_exception_with_message_containing_command_name = () =>
            exception.Message.ShouldContain(typeof(NotConstructingPlainCommand).Name);

        It should_throw_exception_with_message_containing_aggregate_id = () =>
            exception.Message.ShouldContain(aggregateId.FormatGuid());

        private static CommandServiceException exception;
        private static CommandService commandService;
        private static Guid aggregateId = Guid.NewGuid(); // ensure random ID to prevent collisions by NamedLock
    }
}