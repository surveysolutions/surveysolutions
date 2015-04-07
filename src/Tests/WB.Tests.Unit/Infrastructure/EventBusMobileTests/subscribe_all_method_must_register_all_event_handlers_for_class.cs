using Machine.Specifications;
using Moq;
using WB.Core.Infrastructure.Aggregates;
using WB.Core.Infrastructure.EventBus;
using WB.Core.Infrastructure.EventBus.Lite;
using It = Machine.Specifications.It;

namespace WB.Tests.Unit.Infrastructure.EventBusMobileTests
{
    internal class subscribe_all_method_must_register_all_event_handlers_for_class : EventBusTestsContext
    {
        Establish context = () =>
        {
            longValue = 10;
            aggregateRoot = CreateDummyAggregateRoot(longValue);
            
            var eventRegistry = CreateEventRegistry();
            eventBus = CreateEventBus(eventRegistry);

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
        private static DumyEventHandlers handlersClass;
    }
}