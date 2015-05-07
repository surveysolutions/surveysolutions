using System;
using System.Collections.Generic;
using System.Linq;
using FluentAssertions;
using Microsoft.Practices.ServiceLocation;
using Moq;
using Ncqrs.Eventing;
using Ncqrs.Eventing.Sourcing;
using NUnit.Framework;
using Ncqrs.Eventing.Storage;
using WB.Tests.Unit;

namespace Ncqrs.Tests.Eventing.Storage
{
    [TestFixture]
    public class InMemoryEventStoreSpecs
    {
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

            var events = store.ReadFrom(eventSourceId, int.MinValue, int.MaxValue);

            events.Should().NotBeNull();
            events.Should().BeEmpty();
        }

        [Test]
        public void When_getting_all_event_from_an_existing_event_source_the_result_should_be_all_events_stored_for_that_event_source()
        {
            var eventSourceId = Guid.NewGuid();

            var stream1 = new UncommittedEventStream(Guid.NewGuid(), null);
            stream1.Append(new UncommittedEvent(Guid.NewGuid(), eventSourceId, 1, 0, DateTime.UtcNow, new object()));
            stream1.Append(new UncommittedEvent(Guid.NewGuid(), eventSourceId, 2, 0, DateTime.UtcNow, new object()));

            var stream2 = new UncommittedEventStream(Guid.NewGuid(), null);
            stream2.Append(new UncommittedEvent(Guid.NewGuid(), eventSourceId, 3, 1, DateTime.UtcNow, new object()));
            stream2.Append(new UncommittedEvent(Guid.NewGuid(), eventSourceId, 4, 1, DateTime.UtcNow, new object()));
            stream2.Append(new UncommittedEvent(Guid.NewGuid(), eventSourceId, 5, 1, DateTime.UtcNow, new object()));

            var store = new InMemoryEventStore();

            store.Store(stream1);
            store.Store(stream2);

            var events = store.ReadFrom(eventSourceId, int.MinValue, int.MaxValue);

            events.Count().Should().Be(5);
        }
    }
}
