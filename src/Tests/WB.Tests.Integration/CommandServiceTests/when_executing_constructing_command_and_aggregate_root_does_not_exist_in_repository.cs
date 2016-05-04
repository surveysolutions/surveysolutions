using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using FluentAssertions.Events;
using Machine.Specifications;
using Microsoft.Practices.ServiceLocation;
using Moq;
using Ncqrs.Domain;
using Ncqrs.Domain.Storage;
using Ncqrs.Eventing;
using Ncqrs.Eventing.ServiceModel.Bus;
using WB.Core.Infrastructure.Aggregates;
using WB.Core.Infrastructure.CommandBus;
using WB.Core.Infrastructure.CommandBus.Implementation;
using WB.Core.Infrastructure.EventBus;
using WB.Core.Infrastructure.EventBus.Lite;
using IEvent = WB.Core.Infrastructure.EventBus.IEvent;
using It = Machine.Specifications.It;

namespace WB.Tests.Integration.CommandServiceTests
{
    internal class when_executing_constructing_command_and_aggregate_root_does_not_exist_in_repository
    {
        private class Initialize : ICommand { public Guid CommandIdentifier { get; private set; } }
        private class Initialized : IEvent { }

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
            var eventBusMock = Mock.Get(eventBus);

            eventBusMock.Setup(bus => bus.CommitUncommittedEvents(Moq.It.IsAny<IAggregateRoot>(), Moq.It.IsAny<string>()))
                        .Returns((IAggregateRoot aggregate, string origin) =>
                        {
                            constructedAggregateId = aggregate.EventSourceId;
                            return Create.CommittedEventStream(aggregate.EventSourceId, aggregate.GetUnCommittedChanges());
                        });

            eventBusMock
                .Setup(bus => bus.PublishCommittedEvents(Moq.It.IsAny<CommittedEventStream>()))
                .Callback<CommittedEventStream>(events =>
                {
                    publishedEvents = events;
                });

            snapshooterMock = new Mock<IAggregateSnapshotter>();

            IServiceLocator serviceLocator = Mock.Of<IServiceLocator>(_
                => _.GetInstance(typeof(Aggregate)) == new Aggregate());

            commandService = Create.CommandService(repository: repository, eventBus: eventBus, snapshooter: snapshooterMock.Object, serviceLocator: serviceLocator);
        };

        Because of = () =>
            commandService.Execute(new Initialize(), null);

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
        private static CommittedEventStream publishedEvents;
        private static Guid constructedAggregateId;
        private static Mock<IAggregateSnapshotter> snapshooterMock;
    }
}