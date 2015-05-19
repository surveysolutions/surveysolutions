using Machine.Specifications;
using Moq;
using WB.Core.Infrastructure.Aggregates;
using WB.Core.Infrastructure.EventBus.Lite;
using It = Machine.Specifications.It;

namespace WB.Tests.Unit.Infrastructure.LiteEventBusTests
{
    internal class when_publish_event_for_2_event_subscribers_on_this_event : LiteEventBusTestsContext
    {
        Establish context = () =>
        {
            dummyEvent = CreateDummyEvent();
            aggregateRoot = CreateDummyAggregateRoot(dummyEvent, new object());

            var eventRegistry = Create.LiteEventRegistry();
            eventBus = Create.LiteEventBus(eventRegistry);

            sub1Mock = Mock.Of<ILiteEventHandler<DummyEvent>>();
            eventRegistry.Subscribe(sub1Mock);

            sub2Mock = Mock.Of<ILiteEventHandler<DummyEvent>>();
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
        private static ILiteEventHandler<DummyEvent> sub1Mock;
        private static ILiteEventHandler<DummyEvent> sub2Mock;
    }
}