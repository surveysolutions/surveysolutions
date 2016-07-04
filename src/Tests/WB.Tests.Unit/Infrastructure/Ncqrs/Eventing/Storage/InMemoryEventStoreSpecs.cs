using System;
using System.Linq;
using FluentAssertions;
using Ncqrs.Eventing;
using NUnit.Framework;
using Ncqrs.Eventing.Storage;
using IEvent = WB.Core.Infrastructure.EventBus.IEvent;

namespace Ncqrs.Tests.Eventing.Storage
{
    [TestFixture]
    internal class InMemoryEventStoreSpecs
    {
        internal class DummyEvent : IEvent { }

        [SetUp]
        public void SetUp()
        {
            AssemblyContext.SetupServiceLocator();
        }

        public class SomethingDoneEvent
        {
        }

        [Test]
        public void When_getting_all_event_from_a_non_existing_event_source_the_result_should_be_empty()
        {
            var eventSourceId = Guid.NewGuid();
            var store = new InMemoryEventStore();

            var events = store.Read(eventSourceId, int.MinValue);

            events.Should().NotBeNull();
            events.Should().BeEmpty();
        }

        [Test]
        public void When_getting_all_event_from_an_existing_event_source_the_result_should_be_all_events_stored_for_that_event_source()
        {
            var eventSourceId = Guid.NewGuid();

            var stream1 = new UncommittedEventStream(Guid.NewGuid(), null);
            stream1.Append(new UncommittedEvent(Guid.NewGuid(), eventSourceId, 1, 0, DateTime.UtcNow, new DummyEvent()));
            stream1.Append(new UncommittedEvent(Guid.NewGuid(), eventSourceId, 2, 0, DateTime.UtcNow, new DummyEvent()));

            var stream2 = new UncommittedEventStream(Guid.NewGuid(), null);
            stream2.Append(new UncommittedEvent(Guid.NewGuid(), eventSourceId, 3, 1, DateTime.UtcNow, new DummyEvent()));
            stream2.Append(new UncommittedEvent(Guid.NewGuid(), eventSourceId, 4, 1, DateTime.UtcNow, new DummyEvent()));
            stream2.Append(new UncommittedEvent(Guid.NewGuid(), eventSourceId, 5, 1, DateTime.UtcNow, new DummyEvent()));

            var store = new InMemoryEventStore();

            store.Store(stream1);
            store.Store(stream2);

            var events = store.Read(eventSourceId, int.MinValue);

            events.Count().Should().Be(5);
        }
    }
}
