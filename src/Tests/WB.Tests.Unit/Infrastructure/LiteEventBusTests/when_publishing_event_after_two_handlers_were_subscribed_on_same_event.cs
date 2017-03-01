using System;
using Machine.Specifications;
using Moq;
using Ncqrs.Eventing;
using WB.Core.GenericSubdomains.Portable;
using WB.Core.Infrastructure.Aggregates;
using WB.Core.Infrastructure.EventBus.Lite;
using WB.Tests.Abc;
using It = Machine.Specifications.It;

namespace WB.Tests.Unit.Infrastructure.LiteEventBusTests
{
    internal class when_publishing_event_after_two_handlers_were_subscribed_on_same_event : LiteEventBusTestsContext
    {
        Establish context = () =>
        {
            var eventSourceId = Guid.Parse("AAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAA");
            dummyEventStub = CreateDummyEvent();
            eventsToPublish = BuildReadyToBePublishedStream(eventSourceId, dummyEventStub);
 
            var eventRegistry = Create.Service.LiteEventRegistry();
            eventBus = Create.Service.LiteEventBus(eventRegistry);

            firstHandlerMock = new Mock<ILiteEventHandler<DummyEvent>>();
            eventRegistry.Subscribe(firstHandlerMock.Object, eventSourceId.FormatGuid());

            secondHandlerMock = new Mock<ILiteEventHandler<DummyEvent>>();
            eventRegistry.Subscribe(secondHandlerMock.Object, eventSourceId.FormatGuid());
        };

        Because of = () =>
            eventBus.PublishCommittedEvents(eventsToPublish);

        It should_call_Handle_once_for_first_handler = () =>
            firstHandlerMock.Verify(s => s.Handle(dummyEventStub), Times.Once());

        It should_call_Handle_once_for_second_handler = () =>
            secondHandlerMock.Verify(s => s.Handle(dummyEventStub), Times.Once());


        private static ILiteEventBus eventBus;
        private static DummyEvent dummyEventStub;
        private static CommittedEventStream eventsToPublish;
        private static Mock<ILiteEventHandler<DummyEvent>> firstHandlerMock;
        private static Mock<ILiteEventHandler<DummyEvent>> secondHandlerMock;
    }
}