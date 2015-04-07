using Machine.Specifications;
using Moq;
using WB.Core.Infrastructure.Aggregates;
using WB.Core.Infrastructure.EventBus;
using WB.Core.Infrastructure.EventBus.Lite;
using It = Machine.Specifications.It;

namespace WB.Tests.Unit.Infrastructure.EventBusMobileTests
{
    internal class handler_must_not_have_handle_events_after_unsubscribe : EventBusTestsContext
    {
        Establish context = () =>
        {
            aggregateRoot = CreateDummyAggregateRoot((int)10, (long)10, "10");
            
            eventRegistry = CreateEventRegistry();
            eventBus = CreateEventBus(eventRegistry);

            subMock = Mock.Of<IEventBusEventHandler<int>>();
            eventRegistry.Subscribe(subMock);
            eventRegistry.Unsubscribe(subMock);
        };

        Because of = () =>
            eventBus.PublishUncommitedEventsFromAggregateRoot(aggregateRoot, null);

        It should_doesnot_call_Handle_for_int_subscription = () =>
            Mock.Get(subMock).Verify(s => s.Handle(Moq.It.IsAny<int>()), Times.Never);

        private static ILiteEventBus eventBus;
        private static IEventRegistry eventRegistry;
        private static IAggregateRoot aggregateRoot;
        private static IEventBusEventHandler<int> subMock;
    }
}