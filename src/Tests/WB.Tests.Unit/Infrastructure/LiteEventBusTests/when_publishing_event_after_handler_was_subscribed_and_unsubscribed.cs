using System;
using Moq;
using Ncqrs.Eventing;
using WB.Core.Infrastructure.EventBus.Lite;
using WB.Tests.Abc;


namespace WB.Tests.Unit.Infrastructure.LiteEventBusTests
{
    internal class when_publishing_event_after_handler_was_subscribed_and_unsubscribed : LiteEventBusTestsContext
    {
        [NUnit.Framework.OneTimeSetUp] public void context () {
            liteEventRegistry = Create.Service.LiteEventRegistry();
            eventBus = Create.Service.LiteEventBus(liteEventRegistry);
            eventsToPublish = BuildReadyToBePublishedStream(Guid.NewGuid(), new DummyEvent());

            handlerMock = new Mock<ILiteEventHandler<DummyEvent>>();
            liteEventRegistry.Subscribe(handlerMock.Object, "id");
            liteEventRegistry.Unsubscribe(handlerMock.Object);
            BecauseOf();
        }

        public void BecauseOf() =>
            eventBus.PublishCommittedEvents(eventsToPublish);

        [NUnit.Framework.Test] public void should_not_call_Handle_for_this_handler () =>
            handlerMock.Verify(s => s.Handle(Moq.It.IsAny<DummyEvent>()), Times.Never);

        private static ILiteEventBus eventBus;
        private static ILiteEventRegistry liteEventRegistry;
        private static Mock<ILiteEventHandler<DummyEvent>> handlerMock;
        private static CommittedEventStream eventsToPublish;
    }
}
