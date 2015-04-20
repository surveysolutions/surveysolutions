using Machine.Specifications;
using Moq;
using WB.Core.Infrastructure.Aggregates;
using WB.Core.Infrastructure.EventBus.Lite;
using It = Machine.Specifications.It;

namespace WB.Tests.Unit.Infrastructure.LiteEventBusTests
{
    internal class when_publish_long_event_for_int_long_string_subscribes : LiteEventBusTestsContext
    {
        Establish context = () =>
        {
            longValue = 10;
            aggregateRoot = CreateDummyAggregateRoot(longValue);

            var eventRegistry = Create.LiteEventRegistry();
            eventBus = Create.LiteEventBus(eventRegistry);

            handlersClass = CreateDummyClassWithEventHandlers();
            eventRegistry.Subscribe(handlersClass);
        };

        Because of = () =>
            eventBus.PublishUncommitedEventsFromAggregateRoot(aggregateRoot, null);

        It should_doesnot_call_Handle_for_int_subscription = () =>
            Mock.Get(handlersClass).Verify(s => s.Handle(Moq.It.IsAny<int>()), Times.Never);

        It should_call_Handle_for_long_subscription = () =>
            Mock.Get(handlersClass).Verify(s => s.Handle(longValue), Times.Once());

        It should_doesnot_call_Handle_for_string_subscription = () =>
            Mock.Get(handlersClass).Verify(s => s.Handle(Moq.It.IsAny<string>()), Times.Never);


        private static ILiteEventBus eventBus;
        private static long longValue;
        private static IAggregateRoot aggregateRoot;
        private static DumyLiteEventHandlers handlersClass;
    }
}