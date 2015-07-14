using Machine.Specifications;
using Moq;
using WB.Core.Infrastructure.Aggregates;
using WB.Core.Infrastructure.EventBus.Lite;
using It = Machine.Specifications.It;

namespace WB.Tests.Unit.Infrastructure.LiteEventBusTests
{
    internal class when_publishing_event_after_handler_was_subscribed_and_unsubscribed : LiteEventBusTestsContext
    {
        Establish context = () =>
        {
            liteEventRegistry = Create.LiteEventRegistry();
            eventBus = Create.LiteEventBus(liteEventRegistry);
            aggregateRoot = SetupAggregateRootWithOneEventReadyForPublishing<DummyEvent>();

            handlerMock = new Mock<ILiteEventHandler<DummyEvent>>();
            liteEventRegistry.Subscribe(handlerMock.Object, "id");
            liteEventRegistry.Unsubscribe(handlerMock.Object, "id");
        };

        Because of = () =>
            eventBus.PublishUncommitedEventsFromAggregateRoot(aggregateRoot, null);

        It should_not_call_Handle_for_this_handler = () =>
            handlerMock.Verify(s => s.Handle(Moq.It.IsAny<DummyEvent>()), Times.Never);

        private static ILiteEventBus eventBus;
        private static ILiteEventRegistry liteEventRegistry;
        private static IAggregateRoot aggregateRoot;
        private static Mock<ILiteEventHandler<DummyEvent>> handlerMock;
    }
}