using System;
using Moq;
using Ncqrs.Eventing;
using NUnit.Framework;
using WB.Core.Infrastructure.EventBus;
using WB.Core.Infrastructure.EventBus.Lite;
using WB.Tests.Unit;
using IEvent = WB.Core.Infrastructure.EventBus.IEvent;

namespace Ncqrs.Tests.Eventing
{
    [TestFixture]
    internal class UncommittedEventStreamTests
    {
        internal class DummyEvent : IEvent { }

        [SetUp]
        public void SetUp()
        {
            AssemblyContext.SetupServiceLocator();
        }

        [Test]
        public void When_empty_should_indicate_a_single_source()
        {
            var sut = new UncommittedEventStream(Guid.NewGuid(), null);
            Assert.IsTrue(sut.HasSingleSource);
        }

        [Test]
        public void When_contains_single_event_should_indicate_a_single_source()
        {
            var sut = new UncommittedEventStream(Guid.NewGuid(), null);
            sut.Append(new UncommittedEvent(Guid.NewGuid(), Guid.NewGuid(), 0, 0, DateTime.UtcNow, new DummyEvent()));
            Assert.IsTrue(sut.HasSingleSource);
        }

        [Test]
        public void When_contains_multpile_events_from_same_source_should_indicate_a_single_source()
        {
            var sut = new UncommittedEventStream(Guid.NewGuid(), null);
            var eventSourceId = Guid.NewGuid();
            sut.Append(CreateEvent(eventSourceId));
            sut.Append(CreateEvent(eventSourceId));
            Assert.IsTrue(sut.HasSingleSource);
        }

        [Test]
        public void When_contains_multpile_events_from_different_sources_should_indicate_non_single_source()
        {
            var sut = new UncommittedEventStream(Guid.NewGuid(), null);
            sut.Append(CreateEvent(Guid.NewGuid()));
            sut.Append(CreateEvent(Guid.NewGuid()));
            Assert.IsFalse(sut.HasSingleSource);
        }

        private static UncommittedEvent CreateEvent(Guid eventSourceId)
        {
            return new UncommittedEvent(Guid.NewGuid(), eventSourceId, 0, 0, DateTime.UtcNow, new DummyEvent());
        }
    }
}