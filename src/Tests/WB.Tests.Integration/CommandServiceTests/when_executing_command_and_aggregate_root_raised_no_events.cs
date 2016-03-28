using System;
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
    internal class when_executing_command_and_aggregate_root_raised_no_events
    {
        private class DoNothing : ICommand { public Guid CommandIdentifier { get; private set; } }

        private class Aggregate : AggregateRoot
        {
            protected override void HandleEvent(object evnt) { }

            public void DoNothing(DoNothing command) {}
        }

        Establish context = () =>
        {
            CommandRegistry
                .Setup<Aggregate>()
                .Handles<DoNothing>(_ => aggregateId, aggregate => aggregate.DoNothing);

            aggregateFromRepository = new Aggregate();

            var repository = Mock.Of<IAggregateRootRepository>(_
                => _.GetLatest(typeof(Aggregate), aggregateId) == aggregateFromRepository);

            commandService = Create.CommandService(repository: repository, eventBus: eventBusMock.Object, snapshooter: snapshooterMock.Object);
        };

        Because of = () =>
            commandService.Execute(new DoNothing(), null);

        It should_not_commit_events = () =>
            eventBusMock.Verify(
                bus => bus.CommitUncommittedEvents(Moq.It.IsAny<IAggregateRoot>(), Moq.It.IsAny<string>()),
                Times.Never());

        It should_not_publish_events = () =>
            eventBusMock.Verify(
                bus => bus.PublishCommittedEvents(Moq.It.IsAny<CommittedEventStream>()),
                Times.Never());

        private static CommandService commandService;
        private static Guid aggregateId = Guid.Parse("11111111111111111111111111111111");
        private static Mock<IAggregateSnapshotter> snapshooterMock = new Mock<IAggregateSnapshotter>();
        private static Aggregate aggregateFromRepository;
        private static Mock<IEventBus> eventBusMock = new Mock<IEventBus>();
    }
}