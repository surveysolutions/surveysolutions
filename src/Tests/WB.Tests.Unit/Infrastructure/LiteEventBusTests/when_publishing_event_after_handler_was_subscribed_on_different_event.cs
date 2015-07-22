using System;
using Machine.Specifications;
using Moq;
using WB.Core.GenericSubdomains.Portable;
using WB.Core.Infrastructure.Aggregates;
using WB.Core.Infrastructure.EventBus.Lite;
using It = Machine.Specifications.It;

namespace WB.Tests.Unit.Infrastructure.LiteEventBusTests
{
    internal class when_publishing_event_after_handler_was_subscribed_on_different_event : LiteEventBusTestsContext
    {
        Establish context = () =>
        {
            eventStub = CreateDummyEvent();
            var eventSourceId = Guid.Parse("AAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAA");
            aggregateRoot = SetupAggregateRootWithEventReadyForPublishing(eventSourceId, eventStub);

            var eventRegistry = Create.LiteEventRegistry();
            eventBus = Create.LiteEventBus(eventRegistry);

            handlerMock = new Mock<ILiteEventHandler<DifferentDummyEvent>>();
            eventRegistry.Subscribe(handlerMock.Object, eventSourceId.FormatGuid());
        };

        Because of = () =>
            eventBus.PublishUncommitedEventsFromAggregateRoot(aggregateRoot, null);

        It should_not_call_Handle_for_this_handler = () =>
            handlerMock.Verify(s => s.Handle(Moq.It.IsAny<DifferentDummyEvent>()), Times.Never);



        private static ILiteEventBus eventBus;
        private static DummyEvent eventStub;
        private static IAggregateRoot aggregateRoot;
        private static Mock<ILiteEventHandler<DifferentDummyEvent>> handlerMock;
    }
}