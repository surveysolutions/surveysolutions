using System;
using Machine.Specifications;
using Moq;
using Ncqrs.Eventing;
using Ncqrs.Eventing.ServiceModel.Bus;
using WB.Core.Infrastructure.Aggregates;
using WB.Core.Infrastructure.EventBus.Hybrid.Implementation;
using WB.Core.Infrastructure.EventBus.Lite;
using It = Machine.Specifications.It;

namespace WB.Tests.Unit.Infrastructure.HybridEventBusTests
{
    internal class when_publishing_aggregate_root_events_and_publish_to_cqrs_event_bus_fails
    {
        Establish context = () =>
        {
            var cqrsEventBus = Mock.Of<IEventBus>();
            committedEventStream = new CommittedEventStream(Guid.NewGuid());
            Mock.Get(cqrsEventBus)
                .Setup(bus => bus.PublishCommittedEvents(committedEventStream))
                .Throws<Exception>();

            hybridEventBus = Create.Service.HybridEventBus(liteEventBus: liteEventBusMock.Object, cqrsEventBus: cqrsEventBus);
        };

        Because of = () =>
            exception = Catch.Exception(() =>
                hybridEventBus.PublishCommittedEvents(committedEventStream));

        It should_fail = () =>
            exception.ShouldNotBeNull();

        It should_publish_committed_events_to_lite_event_bus = () =>
            liteEventBusMock.Verify(bus => bus.PublishCommittedEvents(committedEventStream), Times.Once);

        private static Exception exception;
        private static HybridEventBus hybridEventBus;

        private static readonly Mock<ILiteEventBus> liteEventBusMock = new Mock<ILiteEventBus>();
        private static CommittedEventStream committedEventStream;
    }
}