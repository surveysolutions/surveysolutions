using System;
using System.Threading;
using Ncqrs.Eventing;
using NUnit.Framework;
using WB.Core.GenericSubdomains.Portable;
using WB.Core.Infrastructure.Implementation.Services;
using WB.Core.SharedKernels.Enumerator.Services.Infrastructure;
using WB.Tests.Abc;
using IEvent = WB.Core.Infrastructure.EventBus.IEvent;

namespace WB.Tests.Unit.Infrastructure
{
    [TestOf(typeof(AsyncEventQueue))]
    internal class AsyncEventQueueTests
    {
        public class DummyHandler : IViewModelEventHandler<DummyEvent>
        {
            public bool WasCalled = false;
            public virtual void Handle(DummyEvent @event) { this.WasCalled = true; }
        }

        public class DifferentDummyHandler : IViewModelEventHandler<DifferentDummyEvent>
        {
            public bool WasCalled = false;
            public virtual void Handle(DifferentDummyEvent @event) { this.WasCalled = true; }
        }

        public class DummyAndDifferentDummyHandler : DummyHandler, IViewModelEventHandler<DifferentDummyEvent>
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
            var viewModelEventQueue = Create.Service.ViewModelEventQueue(eventRegistry);

            var firstHandler = new DummyHandler();
            eventRegistry.Subscribe(firstHandler, eventSourceId.FormatGuid());

            var secondHandler = new DummyHandler();
            eventRegistry.Subscribe(secondHandler, eventSourceId.FormatGuid());

            // act
            viewModelEventQueue.Enqueue(eventsToPublish);
            Thread.Sleep(100);

            // assert
            Assert.That(firstHandler.WasCalled, Is.True);
            Assert.That(secondHandler.WasCalled, Is.True);
        }

        [Test]
        public void when_publishing_event_after_handler_was_subscribed_and_unsubscribed()
        {
            // arrange
            var interviewId = Guid.NewGuid();
            IViewModelEventRegistry eventRegistry = Create.Service.LiteEventRegistry();
            var viewModelEventQueue = Create.Service.ViewModelEventQueue(eventRegistry);
            
            var eventsToPublish = BuildReadyToBePublishedStream(interviewId, new DummyEvent());

            var handler = new DummyHandler();
            eventRegistry.Subscribe(handler, interviewId.ToString("N"));
            eventRegistry.Unsubscribe(handler);

            // act
            viewModelEventQueue.Enqueue(eventsToPublish);
            Thread.Sleep(100);

            // assert
            Assert.That(handler.WasCalled, Is.False);
        }

        [Test]
        public void when_publishing_event_after_handler_was_subscribed_on_different_event()
        {
            // arrange
            var eventStub = CreateDummyEvent();
            var eventSourceId = Guid.Parse("AAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAA");
            var eventsToPublish = BuildReadyToBePublishedStream(eventSourceId, eventStub);

            var eventRegistry = Create.Service.LiteEventRegistry();
            var viewModelEventQueue = Create.Service.ViewModelEventQueue(eventRegistry);

            var handler = new DifferentDummyHandler();
            eventRegistry.Subscribe(handler, eventSourceId.FormatGuid());

            // act
            viewModelEventQueue.Enqueue(eventsToPublish);
            Thread.Sleep(100);

            // assert
            Assert.That(handler.WasCalled, Is.False);
        }

        [Test]
        public void when_publishing_event_after_two_handlers_was_subscribed_on_different_events()
        {
            // arrange
            var eventStub = CreateDummyEvent();
            Guid eventSourceId = Guid.Parse("AAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAA");
            var eventsToPublish = BuildReadyToBePublishedStream(eventSourceId, eventStub);
            var eventRegistry = Create.Service.LiteEventRegistry();
            var viewModelEventQueue = Create.Service.ViewModelEventQueue(eventRegistry);

            var handlerOnFiredEvent = new DummyHandler();
            eventRegistry.Subscribe(handlerOnFiredEvent, eventSourceId.FormatGuid());

            var handlerOnDifferentEvent = new DifferentDummyHandler();
            eventRegistry.Subscribe(handlerOnDifferentEvent, eventSourceId.FormatGuid());

            // act
            viewModelEventQueue.Enqueue(eventsToPublish);
            Thread.Sleep(100);

            // assert
            Assert.That(handlerOnDifferentEvent.WasCalled, Is.False);
            Assert.That(handlerOnFiredEvent.WasCalled, Is.True);
        }

        [Test]
        public void when_publishing_event_declared_in_base_handler_type()
        {
            // arrange
            var eventStub = CreateDummyEvent();
            Guid eventSourceId = Guid.Parse("AAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAA");
            var eventsToPublish = BuildReadyToBePublishedStream(eventSourceId, eventStub);
            var eventRegistry = Create.Service.LiteEventRegistry();
            var viewModelEventQueue = Create.Service.ViewModelEventQueue(eventRegistry);

            var handlerOnFiredEvent = new DummyAndDifferentDummyHandler();
            eventRegistry.Subscribe(handlerOnFiredEvent, eventSourceId.FormatGuid());

            // act
            viewModelEventQueue.Enqueue(eventsToPublish);
            Thread.Sleep(100);

            // assert
            Assert.That(handlerOnFiredEvent.WasCalled, Is.True);
        }
    }
}
