using System;
using System.Collections.Generic;
using FluentAssertions;
using Moq;
using Ncqrs.Eventing;
using Ncqrs.Eventing.ServiceModel.Bus;
using NUnit.Framework;
using WB.Core.GenericSubdomains.Portable;
using WB.Core.Infrastructure.EventBus;
using WB.Core.Infrastructure.EventBus.Lite;
using WB.Core.Infrastructure.Implementation.Services;
using WB.Core.SharedKernels.Enumerator.Services.Infrastructure;
using WB.Tests.Abc;
using IEvent = WB.Core.Infrastructure.EventBus.IEvent;

namespace WB.Tests.Unit.Infrastructure
{
    [TestOf(typeof(LiteEventBus))]
    public class LiteEventBusTests
    {
        public class Denormalizer : BaseDenormalizer, IEventHandler<DummyEvent>
        {
            public bool WasCalled = false;
            public virtual void Handle(IPublishedEvent<DummyEvent> evnt) => this.WasCalled = true;
        }

        public class DifferentDenormalizer : BaseDenormalizer, IEventHandler<DifferentDummyEvent>
        {
            public virtual void Handle(IPublishedEvent<DifferentDummyEvent> evnt) { }
        }

        public class DifferentDenormalizerBasedOnDenormalizer : Denormalizer, IEventHandler<DifferentDummyEvent>
        {
            public virtual void Handle(IPublishedEvent<DifferentDummyEvent> evnt) { }
        }

        public class ViewModel : IViewModelEventHandler<DummyEvent>
        {
            public bool WasCalled = false;
            public virtual void Handle(DummyEvent @event) { this.WasCalled = true; }
        }

        public class DifferentViewModel : IViewModelEventHandler<DifferentDummyEvent>
        {
            public bool WasCalled = false;
            public virtual void Handle(DifferentDummyEvent @event) { this.WasCalled = true; }
        }

        public class DifferentViewModelBasedOnViewModel : ViewModel, IViewModelEventHandler<DifferentDummyEvent>
        {
            public virtual void Handle(DifferentDummyEvent @event) { }
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

            var firstHandlerMock = new Mock<Denormalizer>();
            denormalizerRegistry.RegisterDenormalizer(firstHandlerMock.Object);

            var secondHandlerMock = new Mock<Denormalizer>();
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

            var handlerMock = new Mock<DifferentDenormalizer>();
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

            var handlerOnFiredEventMock = new Mock<Denormalizer>();
            denormalizerRegistry.RegisterDenormalizer(handlerOnFiredEventMock.Object);

            var handlerOnDifferentEventMock = new Mock<DifferentDenormalizer>();
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

            TestDelegate act = () => eventBus.PublishCommittedEvents(eventsToPublish);
            
            Assert.That(act, Throws.Nothing);
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

            var handlerOnFiredEvent = new DifferentDenormalizerBasedOnDenormalizer();

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

            var mockOfViewModelEventQueue = new Mock<IAsyncEventQueue>();
            var eventBus = Create.Service.LiteEventBus(viewModelEventQueue: mockOfViewModelEventQueue.Object);

            // act
            eventBus.PublishCommittedEvents(eventsToPublish);

            // assert
            mockOfViewModelEventQueue.Verify(x => x.Enqueue(It.IsAny<IReadOnlyCollection<CommittedEvent>>()), Times.Once);
        }

        [Test]
        public void when_publishing_event_after_two_view_models_were_subscribed_on_same_event()
        {
            // arrange
            var eventSourceId = Guid.Parse("AAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAA");
            var dummyEventStub = CreateDummyEvent();
            var eventsToPublish = BuildReadyToBePublishedStream(eventSourceId, dummyEventStub);

            var eventRegistry = Create.Service.LiteEventRegistry();
            ILiteEventBus eventBus = Create.Service.LiteEventBus(eventRegistry);

            var firstHandler = new ViewModel();
            eventRegistry.Subscribe(firstHandler, eventSourceId.FormatGuid());

            var secondHandler = new ViewModel();
            eventRegistry.Subscribe(secondHandler, eventSourceId.FormatGuid());

            // act
            eventBus.PublishCommittedEvents(eventsToPublish);

            // assert
            Assert.That(firstHandler.WasCalled, Is.True);
            Assert.That(secondHandler.WasCalled, Is.True);
        }

        [Test]
        public void when_publishing_event_after_view_model_was_subscribed_and_unsubscribed()
        {
            // arrange
            IViewModelEventRegistry liteEventRegistry = Create.Service.LiteEventRegistry();
            ILiteEventBus eventBus = Create.Service.LiteEventBus(liteEventRegistry);
            var eventsToPublish = BuildReadyToBePublishedStream(Guid.NewGuid(), new DummyEvent());

            var handler = new ViewModel();
            liteEventRegistry.Subscribe(handler, "id");
            liteEventRegistry.Unsubscribe(handler);

            // act
            eventBus.PublishCommittedEvents(eventsToPublish);

            // assert
            Assert.That(handler.WasCalled, Is.False);
        }

        [Test]
        public void when_publishing_event_after_view_model_was_subscribed_on_different_event()
        {
            // arrange
            var eventStub = CreateDummyEvent();
            var eventSourceId = Guid.Parse("AAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAA");
            var eventsToPublish = BuildReadyToBePublishedStream(eventSourceId, eventStub);

            var eventRegistry = Create.Service.LiteEventRegistry();
            ILiteEventBus eventBus = Create.Service.LiteEventBus(eventRegistry);

            var handler = new DifferentViewModel();
            eventRegistry.Subscribe(handler, eventSourceId.FormatGuid());

            // act
            eventBus.PublishCommittedEvents(eventsToPublish);

            // assert
            Assert.That(handler.WasCalled, Is.False);
        }

        [Test]
        public void when_publishing_event_after_two_view_models_was_subscribed_on_different_events()
        {
            // arrange
            var eventStub = CreateDummyEvent();
            Guid eventSourceId = Guid.Parse("AAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAA");
            var eventsToPublish = BuildReadyToBePublishedStream(eventSourceId, eventStub);
            var eventRegistry = Create.Service.LiteEventRegistry();
            ILiteEventBus eventBus = Create.Service.LiteEventBus(eventRegistry);

            var handlerOnFiredEvent = new ViewModel();
            eventRegistry.Subscribe(handlerOnFiredEvent, eventSourceId.FormatGuid());

            var handlerOnDifferentEvent = new DifferentViewModel();
            eventRegistry.Subscribe(handlerOnDifferentEvent, eventSourceId.FormatGuid());

            // act
            eventBus.PublishCommittedEvents(eventsToPublish);

            // assert
            Assert.That(handlerOnDifferentEvent.WasCalled, Is.False);
            Assert.That(handlerOnFiredEvent.WasCalled, Is.True);
        }

        [Test]
        public void when_publishing_event_declared_in_base_view_model_type()
        {
            // arrange
            var eventStub = CreateDummyEvent();
            Guid eventSourceId = Guid.Parse("AAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAA");
            var eventsToPublish = BuildReadyToBePublishedStream(eventSourceId, eventStub);
            var eventRegistry = Create.Service.LiteEventRegistry();
            ILiteEventBus eventBus = Create.Service.LiteEventBus(eventRegistry);

            var handlerOnFiredEvent = new DifferentViewModelBasedOnViewModel();
            eventRegistry.Subscribe(handlerOnFiredEvent, eventSourceId.FormatGuid());

            // act
            eventBus.PublishCommittedEvents(eventsToPublish);

            // assert
            handlerOnFiredEvent.WasCalled.Should().BeTrue();
        }

        [Test]
        public void when_PublishCommittedEvents_to_denormalizer_and_view_model_then_committed_events_should_be_published_to_denormalizer_and_view_model()
        {
            // arrange
            var eventStub = CreateDummyEvent();
            Guid eventSourceId = Guid.Parse("AAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAA");
            var eventsToPublish = BuildReadyToBePublishedStream(eventSourceId, eventStub);
            var denormalizerRegistry = Create.Service.DenormalizerRegistry();
            var eventRegistry = Create.Service.LiteEventRegistry();
            ILiteEventBus eventBus = Create.Service.LiteEventBus(denormalizerRegistry: denormalizerRegistry, liteEventRegistry: eventRegistry);

            var denormalizer = new DifferentDenormalizerBasedOnDenormalizer();
            denormalizerRegistry.RegisterDenormalizer(denormalizer);

            var viewModel = new DifferentViewModelBasedOnViewModel();
            eventRegistry.Subscribe(viewModel, eventSourceId.ToString("N"));

            // act
            eventBus.PublishCommittedEvents(eventsToPublish);

            // assert
            Assert.That(denormalizer.WasCalled, Is.True);
            Assert.That(viewModel.WasCalled, Is.True);
        }
    }
}
