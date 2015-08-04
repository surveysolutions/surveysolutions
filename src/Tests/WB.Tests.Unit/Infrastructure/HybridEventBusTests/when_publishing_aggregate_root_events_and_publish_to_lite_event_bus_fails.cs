using System;
using Machine.Specifications;
using Moq;
using Ncqrs.Eventing.ServiceModel.Bus;
using WB.Core.Infrastructure.Aggregates;
using WB.Core.Infrastructure.EventBus.Hybrid.Implementation;
using WB.Core.Infrastructure.EventBus.Lite;
using It = Machine.Specifications.It;

namespace WB.Tests.Unit.Infrastructure.HybridEventBusTests
{
    public class when_publishing_aggregate_root_events_and_publish_to_lite_event_bus_fails
    {
        Establish context = () =>
        {
            var liteEventBus = Mock.Of<ILiteEventBus>();
            Mock.Get(liteEventBus)
                .Setup(bus => bus.PublishUncommittedEvents(aggregateRoot, false))
                .Throws<Exception>();

            hybridEventBus = Create.HybridEventBus(liteEventBus: liteEventBus, cqrsEventBus: cqrsEventBusMock.Object);
        };

        Because of = () =>
            exception = Catch.Exception(() =>
                hybridEventBus.PublishUncommittedEvents(aggregateRoot));

        It should_fail = () =>
            exception.ShouldNotBeNull();

        It should_publish_aggregate_root_events_to_cqrs_event_bus = () =>
            cqrsEventBusMock.Verify(bus => bus.PublishUncommittedEvents(aggregateRoot, false), Times.Once);

        private static Exception exception;
        private static HybridEventBus hybridEventBus;

        private static readonly Mock<IEventBus> cqrsEventBusMock = new Mock<IEventBus>();
        private static readonly IAggregateRoot aggregateRoot = Mock.Of<IAggregateRoot>();
    }
}