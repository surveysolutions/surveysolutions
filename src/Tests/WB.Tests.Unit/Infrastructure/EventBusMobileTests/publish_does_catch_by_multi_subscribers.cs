using Machine.Specifications;
using Moq;
using Ncqrs.Eventing.ServiceModel.Bus;
using WB.Core.Infrastructure.Aggregates;
using WB.Core.Infrastructure.EventBus;
using WB.Core.Infrastructure.EventBus.Lite;
using It = Machine.Specifications.It;

namespace WB.Tests.Unit.Infrastructure.EventBusMobileTests
{
    internal class publish_does_catch_by_multi_subscribers : EventBusTestsContext
    {
        Establish context = () =>
        {
            dummyEvent = CreateDummyEvent();
            aggregateRoot = CreateDummyAggregateRoot(dummyEvent, new object());

            var eventRegistry = CreateEventRegistry();
            eventBus = CreateEventBus(eventRegistry);

            sub1Mock = Mock.Of<IEventBusEventHandler<DummyEvent>>();
            eventRegistry.Subscribe(sub1Mock);

            sub2Mock = Mock.Of<IEventBusEventHandler<DummyEvent>>();
            eventRegistry.Subscribe(sub2Mock);
        };

        Because of = () =>
            eventBus.PublishUncommitedEventsFromAggregateRoot(aggregateRoot, null);

        It should_sub1Mock_call_Handle_once = () =>
            Mock.Get(sub1Mock).Verify(s => s.Handle(dummyEvent), Times.Once());

        It should_sub2Mock_call_Handle_once = () =>
            Mock.Get(sub2Mock).Verify(s => s.Handle(dummyEvent), Times.Once());


        private static ILiteEventBus eventBus;
        private static DummyEvent dummyEvent;
        private static IAggregateRoot aggregateRoot;
        private static IEventBusEventHandler<DummyEvent> sub1Mock;
        private static IEventBusEventHandler<DummyEvent> sub2Mock;
    }
}