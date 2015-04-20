using Machine.Specifications;
using Moq;
using WB.Core.Infrastructure.Aggregates;
using WB.Core.Infrastructure.EventBus.Lite;
using It = Machine.Specifications.It;

namespace WB.Tests.Unit.Infrastructure.LiteEventBusTests
{
    internal class when_publish_event_after_unsubscribe : LiteEventBusTestsContext
    {
        Establish context = () =>
        {
            liteEventRegistry = Create.LiteEventRegistry();
            eventBus = Create.LiteEventBus(liteEventRegistry);
            aggregateRoot = CreateDummyAggregateRoot((int)10, (long)10, "10");

            subMock = CreateDummyClassWithEventHandlers();
            liteEventRegistry.Subscribe(subMock);
            liteEventRegistry.Unsubscribe(subMock);
        };

        Because of = () =>
            eventBus.PublishUncommitedEventsFromAggregateRoot(aggregateRoot, null);

        It should_doesnot_call_Handle_for_int_subscription = () =>
            Mock.Get(subMock).Verify(s => s.Handle(Moq.It.IsAny<int>()), Times.Never);

        It should_doesnot_call_Handle_for_long_subscription = () =>
            Mock.Get(subMock).Verify(s => s.Handle(Moq.It.IsAny<long>()), Times.Never);

        It should_doesnot_call_Handle_for_string_subscription = () =>
            Mock.Get(subMock).Verify(s => s.Handle(Moq.It.IsAny<string>()), Times.Never);

        private static ILiteEventBus eventBus;
        private static ILiteEventRegistry liteEventRegistry;
        private static IAggregateRoot aggregateRoot;
        private static DumyLiteEventHandlers subMock;
    }
}