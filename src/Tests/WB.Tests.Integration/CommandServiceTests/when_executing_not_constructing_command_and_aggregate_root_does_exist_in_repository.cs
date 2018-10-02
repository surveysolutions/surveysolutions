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
using WB.Core.Infrastructure.Aggregates;
using WB.Core.Infrastructure.CommandBus;
using WB.Core.Infrastructure.CommandBus.Implementation;
using IEvent = WB.Core.Infrastructure.EventBus.IEvent;

namespace WB.Tests.Integration.CommandServiceTests
{
    internal class when_executing_not_constructing_command_and_aggregate_root_does_exist_in_repository
    {
        private class Update : ICommand { public Guid CommandIdentifier { get; private set; } }
        private class Updated : IEvent { }

        private class Aggregate : EventSourcedAggregateRoot
        {
            protected override void HandleEvent(object evnt) { }

            public void Update()
            {
                this.ApplyEvent(new Updated());
            }
        }

        [NUnit.Framework.OneTimeSetUp] public void context () {
            CommandRegistry
                .Setup<Aggregate>()
                .Handles<Update>(_ => aggregateId, (command, aggregate) => aggregate.Update());

            aggregateFromRepository = new Aggregate();

            var repository = Mock.Of<IEventSourcedAggregateRootRepository>(_
                => _.GetLatest(typeof(Aggregate), aggregateId) == aggregateFromRepository);

            var eventBus = Mock.Of<IEventBus>();
            var eventBusMock = Mock.Get(eventBus);
            eventBusMock.Setup(bus => bus.CommitUncommittedEvents(Moq.It.IsAny<IEventSourcedAggregateRoot>(), Moq.It.IsAny<string>()))
                     .Returns((IEventSourcedAggregateRoot aggregate, string origin) =>
                     {
                         return new CommittedEventStream(aggregate.EventSourceId,
                             aggregate.GetUnCommittedChanges()
                                      .Select(x => IntegrationCreate.CommittedEvent(payload: x.Payload,
                                                     eventSourceId: x.EventSourceId,
                                                     eventSequence: x.EventSequence)));
                     });

            eventBusMock
                .Setup(bus => bus.PublishCommittedEvents(Moq.It.IsAny<IEnumerable<CommittedEvent>>()))
                .Callback<IEnumerable<CommittedEvent>>(events =>
                {
                    publishedEvents = events;
                });

            snapshooterMock = new Mock<IAggregateSnapshotter>();

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

                        return new CommittedEventStream(eventStream.SourceId, result);
                    }

                    return new CommittedEventStream(eventStream.SourceId);
                });

            commandService = Abc.Create.Service.CommandService(repository: repository, eventBus: eventBus, 
                snapshooter: snapshooterMock.Object, eventStore:eventStore.Object);
            BecauseOf();
        }

        private void BecauseOf() =>
            commandService.Execute(new Update(), null);

        [NUnit.Framework.Test] public void should_publish_result_aggregate_root_event_to_event_bus () =>
            publishedEvents.Single().Payload.Should().BeOfType<Updated>();

        [NUnit.Framework.Test] public void should_create_snapshot_of_aggregate_root_if_needed () =>
            snapshooterMock.Verify(
                snapshooter => snapshooter.CreateSnapshotIfNeededAndPossible(aggregateFromRepository),
                Times.Once());

        private static CommandService commandService;
        private static Guid aggregateId = Guid.NewGuid(); // ensure random ID to prevent collisions by NamedLock
        private static IEnumerable<CommittedEvent> publishedEvents;
        private static Mock<IAggregateSnapshotter> snapshooterMock;
        private static Aggregate aggregateFromRepository;
    }
}
