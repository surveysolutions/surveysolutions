using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
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
    internal class when_executing_not_constructing_command_and_aggregate_root_does_exist_in_repository
    {
        private class Update : ICommand { public Guid CommandIdentifier { get; private set; } }
        private class Updated { }

        private class Aggregate : AggregateRoot
        {
            protected override void HandleEvent(object evnt) { }

            public void Update()
            {
                this.ApplyEvent(new Updated());
            }
        }

        Establish context = () =>
        {
            CommandRegistry
                .Setup<Aggregate>()
                .Handles<Update>(_ => aggregateId, (command, aggregate) => aggregate.Update());

            aggregateFromRepository = new Aggregate();

            var repository = Mock.Of<IAggregateRootRepository>(_
                => _.GetLatest(typeof(Aggregate), aggregateId) == aggregateFromRepository);

            var eventBus = Mock.Of<IEventBus>();
            Mock.Get(eventBus)
                .Setup(bus => bus.PublishUncommitedEventsFromAggregateRoot(aggregateFromRepository, null))
                .Callback<IAggregateRoot, string>((aggregate, origin) =>
                {
                    publishedEvents = aggregate.GetUncommittedChanges();
                });

            snapshooterMock = new Mock<IAggregateSnapshotter>();

            commandService = Create.CommandService(repository: repository, eventBus: eventBus, snapshooter: snapshooterMock.Object);
        };

        Because of = () =>
            commandService.Execute(new Update(), null);

        It should_publish_result_aggregate_root_event_to_event_bus = () =>
            publishedEvents.Single().Payload.ShouldBeOfExactType<Updated>();

        It should_create_snapshot_of_aggregate_root_if_needed = () =>
            snapshooterMock.Verify(
                snapshooter => snapshooter.CreateSnapshotIfNeededAndPossible(aggregateFromRepository),
                Times.Once());

        private static CommandService commandService;
        private static Guid aggregateId = Guid.Parse("11111111111111111111111111111111");
        private static IEnumerable<UncommittedEvent> publishedEvents;
        private static Mock<IAggregateSnapshotter> snapshooterMock;
        private static Aggregate aggregateFromRepository;
    }
}