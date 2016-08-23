using System;
using Machine.Specifications;
using Moq;
using Ncqrs.Eventing;
using WB.Core.GenericSubdomains.Portable;
using WB.Core.Infrastructure.Aggregates;
using WB.Core.Infrastructure.EventBus.Lite;
using It = Machine.Specifications.It;

namespace WB.Tests.Unit.Infrastructure.LiteEventBusTests
{
    public class when_publishing_event_declared_in_base_handler_type : LiteEventBusTestsContext
    {
        public class BaseHandler : ILiteEventHandler<DummyEvent>
        {
            public bool WasCalled = false;
            public virtual void Handle(DummyEvent @event) { this.WasCalled = true; }
        }

        public class ChildrenHandler : BaseHandler, ILiteEventHandler<DifferentDummyEvent>
        {
            public virtual void Handle(DifferentDummyEvent @event) { }
        }

        Establish context = () =>
        {
            eventStub = CreateDummyEvent();
            Guid eventSourceId = Guid.Parse("AAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAA");
            eventsToPublish = BuildReadyToBePublishedStream(eventSourceId, eventStub);
            var eventRegistry = Create.Service.LiteEventRegistry();
            eventBus = Create.Service.LiteEventBus(eventRegistry);

            handlerOnFiredEvent = new ChildrenHandler();
            eventRegistry.Subscribe(handlerOnFiredEvent, eventSourceId.FormatGuid());
        };

        Because of = () =>
            eventBus.PublishCommittedEvents(eventsToPublish);


        It should_call_Handle_once_for_handler_on_current_event = () =>
            handlerOnFiredEvent.WasCalled.ShouldBeTrue();


        private static ILiteEventBus eventBus;
        private static DummyEvent eventStub;
        private static ChildrenHandler handlerOnFiredEvent;
        private static CommittedEventStream eventsToPublish;
    }
}