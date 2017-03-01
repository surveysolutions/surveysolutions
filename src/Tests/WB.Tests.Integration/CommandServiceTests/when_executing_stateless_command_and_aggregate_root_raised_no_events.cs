using System;
using System.Collections.Generic;
using System.Linq;
using Machine.Specifications;
using Moq;
using Ncqrs.Domain;
using Ncqrs.Domain.Storage;
using Ncqrs.Eventing;
using Ncqrs.Eventing.ServiceModel.Bus;
using WB.Core.Infrastructure.Aggregates;
using WB.Core.Infrastructure.CommandBus;
using WB.Core.Infrastructure.CommandBus.Implementation;
using It = Machine.Specifications.It;

namespace WB.Tests.Integration.CommandServiceTests
{
    internal class when_executing_stateless_command_and_aggregate_root_raised_events
    {
        private class DoNothingCommand : ICommand { public Guid CommandIdentifier { get; private set; } }
        private class DoNothingEvent : WB.Core.Infrastructure.EventBus.IEvent
        { 
            public Guid EventIdentifier => Guid.NewGuid();
            public DateTime EventTimeStamp => DateTime.UtcNow;
        }

        private class Aggregate : EventSourcedAggregateRoot
        {
            protected override void HandleEvent(object evnt) { }

            public void DoNothing(DoNothingCommand command)
            {
                ApplyEvent(new DoNothingEvent());
            }
        }

        Establish context = () =>
        {
            CommandRegistry
                .Setup<Aggregate>()
                .StatelessHandles<DoNothingCommand>(_ => aggregateId, aggregate => aggregate.DoNothing);

            aggregateFromRepository = new Aggregate();

            repository = Mock.Of<IEventSourcedAggregateRootRepository>(_
                => _.GetStateless(typeof(Aggregate), aggregateId) == aggregateFromRepository);

            commandService = Abc.Create.Service.CommandService(repository: repository, eventBus: eventBusMock.Object);
        };

        Because of = () =>
            commandService.Execute(new DoNothingCommand(), null);

        It should_commit_events = () =>
            eventBusMock.Verify(
                bus => bus.CommitUncommittedEvents(aggregateFromRepository, null),
                Times.Once);

        It should_publish_events = () =>
            eventBusMock.Verify(
                bus => bus.PublishCommittedEvents(Moq.It.IsAny<IEnumerable<CommittedEvent>>()),
                Times.Once);

        It should_not_load_latest_aggregate_from_repository = () =>
            Mock.Get(repository).Verify(
                repo => repo.GetLatest(Moq.It.IsAny<Type>(), Moq.It.IsAny<Guid>()),
                Times.Never());

        private static CommandService commandService;
        private static IEventSourcedAggregateRootRepository repository;
        private static Guid aggregateId = Guid.NewGuid(); // ensure random ID to prevent collisions by NamedLock
        private static Aggregate aggregateFromRepository;
        private static Mock<IEventBus> eventBusMock = new Mock<IEventBus>();
    }
}