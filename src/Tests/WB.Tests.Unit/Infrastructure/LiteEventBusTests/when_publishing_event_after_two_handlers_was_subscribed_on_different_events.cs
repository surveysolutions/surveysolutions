using System;
using Moq;
using Ncqrs.Eventing;
using WB.Core.GenericSubdomains.Portable;
using WB.Core.Infrastructure.EventBus.Lite;
using WB.Tests.Abc;


namespace WB.Tests.Unit.Infrastructure.LiteEventBusTests
{
    internal class when_publishing_event_after_two_handlers_was_subscribed_on_different_events : LiteEventBusTestsContext
    {
        [NUnit.Framework.OneTimeSetUp] public void context () {
            eventStub = CreateDummyEvent();
            Guid eventSourceId = Guid.Parse("AAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAA");
            eventsToPublish = BuildReadyToBePublishedStream(eventSourceId, eventStub);
            var eventRegistry = Create.Service.LiteEventRegistry();
            eventBus = Create.Service.LiteEventBus(eventRegistry);

            handlerOnFiredEventMock = new Mock<ILiteEventHandler<DummyEvent>>();
            eventRegistry.Subscribe(handlerOnFiredEventMock.Object, eventSourceId.FormatGuid());

            handlerOnDifferentEventMock = new Mock<ILiteEventHandler<DifferentDummyEvent>>();
            eventRegistry.Subscribe(handlerOnDifferentEventMock.Object, eventSourceId.FormatGuid());
            BecauseOf();
        }

        public void BecauseOf() =>
            eventBus.PublishCommittedEvents(eventsToPublish);

        [NUnit.Framework.Test] public void should_not_call_Handle_for_handler_assigned_on_different_event () =>
            handlerOnDifferentEventMock.Verify(s => s.Handle(Moq.It.IsAny<DifferentDummyEvent>()), Times.Never);

        [NUnit.Framework.Test] public void should_call_Handle_once_for_handler_on_current_event () =>
            handlerOnFiredEventMock.Verify(s => s.Handle(eventStub), Times.Once());


        private static ILiteEventBus eventBus;
        private static DummyEvent eventStub;
        private static Mock<ILiteEventHandler<DummyEvent>> handlerOnFiredEventMock;
        private static Mock<ILiteEventHandler<DifferentDummyEvent>> handlerOnDifferentEventMock;
        private static CommittedEventStream eventsToPublish;
    }
}
