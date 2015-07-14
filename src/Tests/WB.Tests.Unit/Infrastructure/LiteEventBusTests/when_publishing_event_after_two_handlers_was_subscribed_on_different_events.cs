using Machine.Specifications;
using Moq;
using WB.Core.Infrastructure.Aggregates;
using WB.Core.Infrastructure.EventBus.Lite;
using It = Machine.Specifications.It;

namespace WB.Tests.Unit.Infrastructure.LiteEventBusTests
{
    internal class when_publishing_event_after_two_handlers_was_subscribed_on_different_events : LiteEventBusTestsContext
    {
        Establish context = () =>
        {
            eventStub = CreateDummyEvent();
            aggregateRoot = SetupAggregateRootWithEventReadyForPublishing(eventStub);
            var eventRegistry = Create.LiteEventRegistry();
            eventBus = Create.LiteEventBus(eventRegistry);

            handlerOnFiredEventMock = new Mock<ILiteEventHandler<DummyEvent>>();
            eventRegistry.Subscribe(handlerOnFiredEventMock.Object, "id");

            handlerOnDifferentEventMock = new Mock<ILiteEventHandler<DifferentDummyEvent>>();
            eventRegistry.Subscribe(handlerOnDifferentEventMock.Object, "id");
        };

        Because of = () =>
            eventBus.PublishUncommitedEventsFromAggregateRoot(aggregateRoot, null);

        It should_not_call_Handle_for_handler_assigned_on_different_event = () =>
            handlerOnDifferentEventMock.Verify(s => s.Handle(Moq.It.IsAny<DifferentDummyEvent>()), Times.Never);

        It should_call_Handle_once_for_handler_on_current_event = () =>
            handlerOnFiredEventMock.Verify(s => s.Handle(eventStub), Times.Once());


        private static ILiteEventBus eventBus;
        private static DummyEvent eventStub;
        private static IAggregateRoot aggregateRoot;
        private static Mock<ILiteEventHandler<DummyEvent>> handlerOnFiredEventMock;
        private static Mock<ILiteEventHandler<DifferentDummyEvent>> handlerOnDifferentEventMock;
    }
}