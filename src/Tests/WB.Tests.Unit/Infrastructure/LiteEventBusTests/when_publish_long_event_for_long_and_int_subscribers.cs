using Machine.Specifications;
using Moq;
using WB.Core.Infrastructure.Aggregates;
using WB.Core.Infrastructure.EventBus.Lite;
using It = Machine.Specifications.It;

namespace WB.Tests.Unit.Infrastructure.LiteEventBusTests
{
    internal class when_publish_long_event_for_long_and_int_subscribers : LiteEventBusTestsContext
    {
        Establish context = () =>
        {
            longValue = 10;
            aggregateRoot = CreateDummyAggregateRoot(longValue);
            var eventRegistry = Create.LiteEventRegistry();
            eventBus = Create.LiteEventBus(eventRegistry);

            sub1Mock = Mock.Of<ILiteEventHandler<int>>();
            eventRegistry.Subscribe(sub1Mock);

            sub2Mock = Mock.Of<ILiteEventHandler<long>>();
            eventRegistry.Subscribe(sub2Mock);
        };

        Because of = () =>
            eventBus.PublishUncommitedEventsFromAggregateRoot(aggregateRoot, null);

        It should_sub1Mock_doesnot_call_Handle = () =>
            Mock.Get(sub1Mock).Verify(s => s.Handle(Moq.It.IsAny<int>()), Times.Never);

        It should_sub2Mock_call_Handle_once = () =>
            Mock.Get(sub2Mock).Verify(s => s.Handle(longValue), Times.Once());


        private static ILiteEventBus eventBus;
        private static long longValue;
        private static IAggregateRoot aggregateRoot;
        private static ILiteEventHandler<int> sub1Mock;
        private static ILiteEventHandler<long> sub2Mock;
    }
}