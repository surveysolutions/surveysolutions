using System;
using Moq;
using Ncqrs.Domain;
using Ncqrs.Domain.Storage;
using Ncqrs.Eventing;
using Ncqrs.Eventing.ServiceModel.Bus;
using Ncqrs.Eventing.Storage;
using NUnit.Framework;
using WB.Core.Infrastructure.Aggregates;
using WB.Core.Infrastructure.CommandBus;
using WB.Core.Infrastructure.CommandBus.Implementation;
using WB.Tests.Abc;

namespace WB.Tests.Integration.CommandServiceTests
{
    internal class when_executing_command_and_aggregate_root_raised_no_events
    {
        private class DoNothing : ICommand { public Guid CommandIdentifier { get; private set; } }

        private class Aggregate : EventSourcedAggregateRoot
        {
            protected override void HandleEvent(object evnt) { }

            public void DoNothing(DoNothing command) {}
        }

        [OneTimeSetUp]
        public void Context()
        {
            CommandRegistry
                .Setup<Aggregate>()
                .Handles<DoNothing>(_ => aggregateId, aggregate => aggregate.DoNothing);

            aggregateFromRepository = new Aggregate();

            var repository = Mock.Of<IEventSourcedAggregateRootRepository>(_
                => _.GetLatest(typeof(Aggregate), aggregateId) == aggregateFromRepository);

            commandService = Create.Service.CommandService(repository: repository, eventBus: eventBusMock.Object, snapshooter: snapshooterMock.Object, 
                eventStore: eventStoreMock.Object);

            // Act
            commandService.Execute(new DoNothing(), null);
        }

        [Test]
        public void should_not_commit_events() =>
            eventStoreMock.Verify(
                eventStore => eventStore.Store(Moq.It.IsAny<UncommittedEventStream>()),
                Times.Never());
        [Test]
        public void should_not_publish_events() =>
            eventBusMock.Verify(
                bus => bus.PublishCommittedEvents(Moq.It.IsAny<CommittedEventStream>()),
                Times.Never());

        private static CommandService commandService;
        private static Guid aggregateId = Guid.NewGuid(); // ensure random ID to prevent collisions by NamedLock
        private static Mock<IAggregateSnapshotter> snapshooterMock = new Mock<IAggregateSnapshotter>();
        private static Aggregate aggregateFromRepository;
        private static Mock<IEventBus> eventBusMock = new Mock<IEventBus>();
        private static Mock<IEventStore> eventStoreMock = new Mock<IEventStore>();
    }
}
