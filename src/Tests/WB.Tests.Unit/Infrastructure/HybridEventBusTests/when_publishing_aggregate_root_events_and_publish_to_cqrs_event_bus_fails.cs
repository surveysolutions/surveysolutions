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
    internal class when_publishing_aggregate_root_events_and_publish_to_cqrs_event_bus_fails
    {
        Establish context = () =>
        {
            var cqrsEventBus = Mock.Of<IEventBus>();
            Mock.Get(cqrsEventBus)
                .Setup(bus => bus.PublishUncommittedEvents(aggregateRoot))
                .Throws<Exception>();

            hybridEventBus = Create.HybridEventBus(liteEventBus: liteEventBusMock.Object, cqrsEventBus: cqrsEventBus);
        };

        Because of = () =>
            exception = Catch.Exception(() =>
                hybridEventBus.PublishUncommittedEvents(aggregateRoot));

        It should_fail = () =>
            exception.ShouldNotBeNull();

        It should_publish_aggregate_root_events_to_lite_event_bus = () =>
            liteEventBusMock.Verify(bus => bus.PublishUncommittedEvents(aggregateRoot), Times.Once);

        private static Exception exception;
        private static HybridEventBus hybridEventBus;

        private static readonly Mock<ILiteEventBus> liteEventBusMock = new Mock<ILiteEventBus>();
        private static readonly IAggregateRoot aggregateRoot = Mock.Of<IAggregateRoot>();
    }
}