using System;
using System.Collections.Generic;
using System.Linq;
using FluentAssertions;
using Moq;
using Ncqrs.Domain;
using Ncqrs.Domain.Storage;
using Ncqrs.Eventing;
using Ncqrs.Eventing.ServiceModel.Bus;
using Ncqrs.Eventing.Storage;
using WB.Core.GenericSubdomains.Portable.ServiceLocation;
using WB.Core.Infrastructure.Aggregates;
using WB.Core.Infrastructure.CommandBus;
using WB.Core.Infrastructure.CommandBus.Implementation;
using IEvent = WB.Core.Infrastructure.EventBus.IEvent;

namespace WB.Tests.Integration.CommandServiceTests
{
    internal class when_executing_constructing_command_and_aggregate_root_does_not_exist_in_repository
    {
        private class Initialize : ICommand { public Guid CommandIdentifier { get; private set; } }
        private class Initialized : IEvent { }

        private class Aggregate : EventSourcedAggregateRoot
        {
            protected override void HandleEvent(object evnt) { }

            public void Initialize()
            {
                this.ApplyEvent(new Initialized());
            }
        }

        [NUnit.Framework.OneTimeSetUp] public void context () {
            CommandRegistry
                .Setup<Aggregate>()
                .InitializesWith<Initialize>(_ => aggregateId, (command, aggregate) => aggregate.Initialize());

            var repository = Mock.Of<IEventSourcedAggregateRootRepository>(_
                => _.GetLatest(typeof(Aggregate), aggregateId) == null as Aggregate);

            var eventBus = Mock.Of<IEventBus>();
            var eventBusMock = Mock.Get(eventBus);

            eventBusMock
                .Setup(bus => bus.PublishCommittedEvents(Moq.It.IsAny<IEnumerable<CommittedEvent>>()))
                .Callback<IEnumerable<CommittedEvent>>(events =>
                {
                    publishedEvents = events;
                });

            snapshooterMock = new Mock<IAggregateSnapshotter>();

            IServiceLocator serviceLocator = Mock.Of<IServiceLocator>(_
                => _.GetInstance(typeof(Aggregate)) == new Aggregate());

            var eventStore = new Mock<IEventStore>();
            eventStore.Setup(x => x.Store(Moq.It.IsAny<UncommittedEventStream>())).Returns(
                (UncommittedEventStream eventStream) =>
                {
                    if (eventStream.IsNotEmpty)
                    {
                        List<CommittedEvent> result = new List<CommittedEvent>();

                        var events = new Queue<CommittedEvent>();

                        foreach (var evnt in eventStream)
                        {
                            var committedEvent = new CommittedEvent(eventStream.CommitId,
                                evnt.Origin,
                                evnt.EventIdentifier,
                                eventStream.SourceId,
                                evnt.EventSequence,
                                evnt.EventTimeStamp,
                                events.Count,
                                evnt.Payload);
                            events.Enqueue(committedEvent);
                            result.Add(committedEvent);
                        }

                        constructedAggregateId = eventStream.SourceId;

                        return new CommittedEventStream(eventStream.SourceId, result);
                    }

                    return new CommittedEventStream(eventStream.SourceId);
                });


            commandService = Abc.Create.Service.CommandService(repository: repository, eventBus: eventBus, 
                snapshooter: snapshooterMock.Object, serviceLocator: serviceLocator, eventStore : eventStore.Object);

            BecauseOf();
        }

        private void BecauseOf() =>
            commandService.Execute(new Initialize(), null);

        [NUnit.Framework.Test] public void should_publish_result_aggregate_root_event_to_event_bus () =>
            publishedEvents.Single().Payload.Should().BeOfType<Initialized>();

        [NUnit.Framework.Test] public void should_set_specified_aggregate_id_to_constructed_aggregate_root () =>
            constructedAggregateId.Should().Be(aggregateId);

        [NUnit.Framework.Test] public void should_create_snapshot_of_aggregate_root_if_needed () =>
            snapshooterMock.Verify(
                snapshooter => snapshooter.CreateSnapshotIfNeededAndPossible(Moq.It.Is<IEventSourcedAggregateRoot>(aggregate => aggregate.EventSourceId == aggregateId)),
                Times.Once());

        private static CommandService commandService;
        private static Guid aggregateId = Guid.NewGuid(); // ensure random ID to prevent collisions by NamedLock
        private static IEnumerable<CommittedEvent> publishedEvents;
        private static Guid constructedAggregateId;
        private static Mock<IAggregateSnapshotter> snapshooterMock;
    }
}
