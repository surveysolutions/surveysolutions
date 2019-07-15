using System;
using FluentAssertions;
using Moq;
using Ncqrs.Eventing;
using NUnit.Framework;
using WB.Core.GenericSubdomains.Portable;
using WB.Core.GenericSubdomains.Portable.Services;
using WB.Core.GenericSubdomains.Portable.Tasks;
using WB.Core.Infrastructure.EventBus.Lite;
using WB.Core.SharedKernels.Enumerator.Services.Infrastructure;
using WB.Tests.Abc;
using IEvent = WB.Core.Infrastructure.EventBus.IEvent;

namespace WB.Tests.Unit.Infrastructure
{
    [TestOf(typeof(LiteEventBus))]
    public class LiteEventBusTests
    {
        public class BaseHandler : IViewModelEventHandler<DummyEvent>
        {
            public bool WasCalled = false;
            public virtual void Handle(DummyEvent @event) { this.WasCalled = true; }
        }

        public class ChildrenHandler : BaseHandler, IViewModelEventHandler<DifferentDummyEvent>
        {
            public virtual void Handle(DifferentDummyEvent @event) { }
        }

        public class DummyEvent : IEvent { }

        public class DifferentDummyEvent : IEvent { }

        internal static DummyEvent CreateDummyEvent()
        {
            return new DummyEvent();
        }

        protected static CommittedEventStream BuildReadyToBePublishedStream(Guid eventSourceId, IEvent @event)
        {
            return new CommittedEventStream(eventSourceId,
                Create.Other.CommittedEvent(eventSourceId: eventSourceId, payload: @event));
        }

        [Test]
        public void when_publishing_event_after_two_handlers_were_subscribed_on_same_event()
        {
            // arrange
            var eventSourceId = Guid.Parse("AAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAA");
            var dummyEventStub = CreateDummyEvent();
            var eventsToPublish = BuildReadyToBePublishedStream(eventSourceId, dummyEventStub);

            var eventRegistry = Create.Service.LiteEventRegistry();
            ILiteEventBus eventBus = Create.Service.LiteEventBus(eventRegistry);

            var firstHandlerMock = new Mock<IViewModelEventHandler<DummyEvent>>();
            eventRegistry.Subscribe(firstHandlerMock.Object, eventSourceId.FormatGuid());

            var secondHandlerMock = new Mock<IViewModelEventHandler<DummyEvent>>();
            eventRegistry.Subscribe(secondHandlerMock.Object, eventSourceId.FormatGuid());

            // act
            eventBus.PublishCommittedEvents(eventsToPublish);

            // assert
            firstHandlerMock.Verify(s => s.Handle(dummyEventStub), Times.Once());
            secondHandlerMock.Verify(s => s.Handle(dummyEventStub), Times.Once());
        }

        [Test]
        public void when_publishing_event_after_handler_was_subscribed_and_unsubscribed()
        {
            // arrange
            IViewModelEventRegistry liteEventRegistry = Create.Service.LiteEventRegistry();
            ILiteEventBus eventBus = Create.Service.LiteEventBus(liteEventRegistry);
            var eventsToPublish = BuildReadyToBePublishedStream(Guid.NewGuid(), new DummyEvent());

            var handlerMock = new Mock<IViewModelEventHandler<DummyEvent>>();
            liteEventRegistry.Subscribe(handlerMock.Object, "id");
            liteEventRegistry.Unsubscribe(handlerMock.Object);

            // act
            eventBus.PublishCommittedEvents(eventsToPublish);

            // assert
            handlerMock.Verify(s => s.Handle(Moq.It.IsAny<DummyEvent>()), Times.Never);
        }

        [Test]
        public void when_publishing_event_after_handler_was_subscribed_on_different_event()
        {
            // arrange
            var eventStub = CreateDummyEvent();
            var eventSourceId = Guid.Parse("AAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAA");
            var eventsToPublish = BuildReadyToBePublishedStream(eventSourceId, eventStub);

            var eventRegistry = Create.Service.LiteEventRegistry();
            ILiteEventBus eventBus = Create.Service.LiteEventBus(eventRegistry);

            var handlerMock = new Mock<IViewModelEventHandler<DifferentDummyEvent>>();
            eventRegistry.Subscribe(handlerMock.Object, eventSourceId.FormatGuid());

            // act
            eventBus.PublishCommittedEvents(eventsToPublish);

            // assert
            handlerMock.Verify(s => s.Handle(Moq.It.IsAny<DifferentDummyEvent>()), Times.Never);
        }

        [Test]
        public void when_publishing_event_after_two_handlers_was_subscribed_on_different_events()
        {
            // arrange
            var eventStub = CreateDummyEvent();
            Guid eventSourceId = Guid.Parse("AAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAA");
            var eventsToPublish = BuildReadyToBePublishedStream(eventSourceId, eventStub);
            var eventRegistry = Create.Service.LiteEventRegistry();
            ILiteEventBus eventBus = Create.Service.LiteEventBus(eventRegistry);

            var handlerOnFiredEventMock = new Mock<IViewModelEventHandler<DummyEvent>>();
            eventRegistry.Subscribe(handlerOnFiredEventMock.Object, eventSourceId.FormatGuid());

            var handlerOnDifferentEventMock = new Mock<IViewModelEventHandler<DifferentDummyEvent>>();
            eventRegistry.Subscribe(handlerOnDifferentEventMock.Object, eventSourceId.FormatGuid());

            // act
            eventBus.PublishCommittedEvents(eventsToPublish);

            // assert
            handlerOnDifferentEventMock.Verify(s => s.Handle(Moq.It.IsAny<DifferentDummyEvent>()), Times.Never);
            handlerOnFiredEventMock.Verify(s => s.Handle(eventStub), Times.Once());
        }

        [Test]
        public void when_publishing_event_and_bus_does_not_have_any_subscriptions()
        {
            var eventsToPublish = BuildReadyToBePublishedStream(Guid.NewGuid(), new DummyEvent());

            var eventBus = Create.Service.LiteEventBus();

            Assert.That(() => eventBus.PublishCommittedEvents(eventsToPublish), Throws.Nothing);
        }

        [Test]
        public void when_publishing_event_declared_in_base_handler_type()
        {
            // arrange
            var eventStub = CreateDummyEvent();
            Guid eventSourceId = Guid.Parse("AAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAA");
            var eventsToPublish = BuildReadyToBePublishedStream(eventSourceId, eventStub);
            var eventRegistry = Create.Service.LiteEventRegistry();
            ILiteEventBus eventBus = Create.Service.LiteEventBus(eventRegistry);

            var handlerOnFiredEvent = new ChildrenHandler();
            eventRegistry.Subscribe(handlerOnFiredEvent, eventSourceId.FormatGuid());

            // act
            eventBus.PublishCommittedEvents(eventsToPublish);

            // assert
            handlerOnFiredEvent.WasCalled.Should().BeTrue();
        }
    }
}
