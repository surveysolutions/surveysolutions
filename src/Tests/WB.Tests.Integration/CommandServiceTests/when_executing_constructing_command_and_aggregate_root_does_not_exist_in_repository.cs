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
using WB.Core.Infrastructure.Implementation.CommandBus;
using It = Machine.Specifications.It;

namespace WB.Tests.Integration.CommandServiceTests
{
    internal class when_executing_constructing_command_and_aggregate_root_does_not_exist_in_repository
    {
        private class Initialize : ICommand { public Guid CommandIdentifier { get; private set; } }
        private class Initialized {}

        private class Aggregate : AggregateRoot
        {
            protected override void HandleEvent(object evnt) { }

            public void Initialize()
            {
                this.ApplyEvent(new Initialized());
            }
        }

        Establish context = () =>
        {
            CommandRegistry
                .Setup<Aggregate>()
                .InitializesWith<Initialize>(_ => aggregateId, (command, aggregate) => aggregate.Initialize());

            var repository = Mock.Of<IAggregateRootRepository>(_
                => _.GetLatest(typeof(Aggregate), aggregateId) == null as Aggregate);

            var eventBus = Mock.Of<IEventBus>();
            Mock.Get(eventBus)
                .Setup(bus => bus.PublishUncommitedEventsFromAggregateRoot(Moq.It.IsAny<IAggregateRoot>(), null, false))
                .Callback<IAggregateRoot, string, bool>((aggregate, origin, inBatch) =>
                {
                    publishedEvents = aggregate.GetUncommittedChanges();
                    constructedAggregateId = aggregate.EventSourceId;
                });

            snapshooterMock = new Mock<IAggregateSnapshotter>();

            commandService = Create.CommandService(repository: repository, eventBus: eventBus, snapshooter: snapshooterMock.Object);
        };

        Because of = () =>
            commandService.Execute(new Initialize(), null, false);

        It should_publish_result_aggregate_root_event_to_event_bus = () =>
            publishedEvents.Single().Payload.ShouldBeOfExactType<Initialized>();

        It should_set_specified_aggregate_id_to_constructed_aggregate_root = () =>
            constructedAggregateId.ShouldEqual(aggregateId);

        It should_create_snapshot_of_aggregate_root_if_needed = () =>
            snapshooterMock.Verify(
                snapshooter => snapshooter.CreateSnapshotIfNeededAndPossible(Moq.It.Is<IAggregateRoot>(aggregate => aggregate.EventSourceId == aggregateId)),
                Times.Once());

        private static CommandService commandService;
        private static Guid aggregateId = Guid.Parse("11111111111111111111111111111111");
        private static IEnumerable<UncommittedEvent> publishedEvents;
        private static Guid constructedAggregateId;
        private static Mock<IAggregateSnapshotter> snapshooterMock;
    }
}