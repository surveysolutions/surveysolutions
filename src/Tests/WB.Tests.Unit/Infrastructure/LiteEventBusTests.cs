using System;
using System.Collections.Generic;
using FluentAssertions;
using Moq;
using Ncqrs.Eventing;
using Ncqrs.Eventing.ServiceModel.Bus;
using NUnit.Framework;
using WB.Core.Infrastructure.EventBus;
using WB.Core.Infrastructure.EventBus.Lite;
using WB.Core.SharedKernels.Enumerator.Services.Infrastructure;
using WB.Tests.Abc;
using IEvent = WB.Core.Infrastructure.EventBus.IEvent;

namespace WB.Tests.Unit.Infrastructure
{
    [TestOf(typeof(LiteEventBus))]
    public class LiteEventBusTests
    {
        public class DummyDenormalizer : BaseDenormalizer, IEventHandler<DummyEvent>
        {
            public bool WasCalled = false;
            public virtual void Handle(IPublishedEvent<DummyEvent> evnt) => this.WasCalled = true;
            public override object[] Writers { get; }
        }

        public class DifferentDummyDenormalizer : BaseDenormalizer, IEventHandler<DifferentDummyEvent>
        {
            public virtual void Handle(IPublishedEvent<DifferentDummyEvent> evnt) { }
            public override object[] Writers { get; }
        }

        public class ChildrenHandler : DummyDenormalizer, IEventHandler<DifferentDummyEvent>
        {
            public virtual void Handle(IPublishedEvent<DifferentDummyEvent> evnt) { }
        }

        public class DummyEvent : IEvent { }

        public class DifferentDummyEvent : IEvent { }

        internal static DummyEvent CreateDummyEvent() => new DummyEvent();

        protected static CommittedEventStream BuildReadyToBePublishedStream(Guid eventSourceId, IEvent @event) =>
            new CommittedEventStream(eventSourceId,
                Create.Other.CommittedEvent(eventSourceId: eventSourceId, payload: @event));

        [Test]
        public void when_publishing_event_after_two_handlers_were_subscribed_on_same_event()
        {
            // arrange
            var eventSourceId = Guid.Parse("AAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAA");
            var dummyEventStub = CreateDummyEvent();
            var eventsToPublish = BuildReadyToBePublishedStream(eventSourceId, dummyEventStub);

            var denormalizerRegistry = Create.Service.DenormalizerRegistry();
            ILiteEventBus eventBus = Create.Service.LiteEventBus(denormalizerRegistry: denormalizerRegistry);

            var firstHandlerMock = new Mock<DummyDenormalizer>();
            denormalizerRegistry.RegisterDenormalizer(firstHandlerMock.Object);

            var secondHandlerMock = new Mock<DummyDenormalizer>();
            denormalizerRegistry.RegisterDenormalizer(secondHandlerMock.Object);

            // act
            eventBus.PublishCommittedEvents(eventsToPublish);

            // assert
            firstHandlerMock.Verify(s => s.Handle(It.Is<IPublishedEvent<DummyEvent>>(x => x.Payload == dummyEventStub)), Times.Once());
            secondHandlerMock.Verify(s => s.Handle(It.Is<IPublishedEvent<DummyEvent>>(x => x.Payload == dummyEventStub)), Times.Once());
        }

        [Test]
        public void when_publishing_event_after_handler_was_subscribed_on_different_event()
        {
            // arrange
            var eventStub = CreateDummyEvent();
            var eventSourceId = Guid.Parse("AAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAA");
            var eventsToPublish = BuildReadyToBePublishedStream(eventSourceId, eventStub);

            var denormalizerRegistry = Create.Service.DenormalizerRegistry();
            ILiteEventBus eventBus = Create.Service.LiteEventBus(denormalizerRegistry: denormalizerRegistry);

            var handlerMock = new Mock<DifferentDummyDenormalizer>();
            denormalizerRegistry.RegisterDenormalizer(handlerMock.Object);

            // act
            eventBus.PublishCommittedEvents(eventsToPublish);

            // assert
            handlerMock.Verify(s => s.Handle(Moq.It.IsAny<IPublishedEvent<DifferentDummyEvent>>()), Times.Never);
        }

        [Test]
        public void when_publishing_event_after_two_handlers_was_subscribed_on_different_events()
        {
            // arrange
            var eventStub = CreateDummyEvent();
            Guid eventSourceId = Guid.Parse("AAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAA");
            var eventsToPublish = BuildReadyToBePublishedStream(eventSourceId, eventStub);
            var denormalizerRegistry = Create.Service.DenormalizerRegistry();
            ILiteEventBus eventBus = Create.Service.LiteEventBus(denormalizerRegistry: denormalizerRegistry);

            var handlerOnFiredEventMock = new Mock<DummyDenormalizer>();
            denormalizerRegistry.RegisterDenormalizer(handlerOnFiredEventMock.Object);

            var handlerOnDifferentEventMock = new Mock<DifferentDummyDenormalizer>();
            denormalizerRegistry.RegisterDenormalizer(handlerOnDifferentEventMock.Object);

            // act
            eventBus.PublishCommittedEvents(eventsToPublish);

            // assert
            handlerOnDifferentEventMock.Verify(s => s.Handle(Moq.It.IsAny<IPublishedEvent<DifferentDummyEvent>>()), Times.Never);
            handlerOnFiredEventMock.Verify(s => s.Handle(It.Is<IPublishedEvent<DummyEvent>>(x=>x.Payload == eventStub)), Times.Once());
        }

        [Test]
        public void when_publishing_event_and_bus_does_not_have_any_subscriptions()
        {
            var eventsToPublish = BuildReadyToBePublishedStream(Guid.NewGuid(), new DummyEvent());
            var denormalizerRegistry = Create.Service.DenormalizerRegistry();
            var eventBus = Create.Service.LiteEventBus(denormalizerRegistry: denormalizerRegistry);

            Assert.That(() => eventBus.PublishCommittedEvents(eventsToPublish), Throws.Nothing);
        }

        [Test]
        public void when_publishing_event_declared_in_base_handler_type()
        {
            // arrange
            var eventStub = CreateDummyEvent();
            Guid eventSourceId = Guid.Parse("AAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAA");
            var eventsToPublish = BuildReadyToBePublishedStream(eventSourceId, eventStub);
            var denormalizerRegistry = Create.Service.DenormalizerRegistry();
            ILiteEventBus eventBus = Create.Service.LiteEventBus(denormalizerRegistry: denormalizerRegistry);

            var handlerOnFiredEvent = new ChildrenHandler();

            denormalizerRegistry.RegisterDenormalizer(handlerOnFiredEvent);

            // act
            eventBus.PublishCommittedEvents(eventsToPublish);

            // assert
            handlerOnFiredEvent.WasCalled.Should().BeTrue();
        }

        [Test]
        public void when_PublishCommittedEvents_then_committed_events_should_be_added_to_queue()
        {
            // arrange
            var eventsToPublish = BuildReadyToBePublishedStream(Guid.Parse("AAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAA"), CreateDummyEvent());

            var mockOfViewModelEventQueue = new Mock<IViewModelEventQueue>();
            var eventBus = Create.Service.LiteEventBus(viewModelEventQueue: mockOfViewModelEventQueue.Object);

            // act
            eventBus.PublishCommittedEvents(eventsToPublish);

            // assert
            mockOfViewModelEventQueue.Verify(x => x.Enqueue(It.IsAny<IEnumerable<CommittedEvent>>()), Times.Once);
        }
    }
}
