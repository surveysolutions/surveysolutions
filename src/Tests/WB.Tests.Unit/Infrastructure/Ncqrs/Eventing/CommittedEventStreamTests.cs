using System;
using System.Collections.Generic;
using System.Linq;
using FluentAssertions;
using Moq;
using Ncqrs.Eventing;
using NUnit.Framework;
using Ncqrs.Spec;
using WB.Core.Infrastructure.EventBus;
using WB.Core.Infrastructure.EventBus.Lite;
using WB.Tests.Unit;
using IEvent = WB.Core.Infrastructure.EventBus.IEvent;

namespace Ncqrs.Tests.Eventing
{
    [TestFixture]
    internal class CommittedEventStreamTests
    {
        internal class DummyEvent : IEvent { }

        [SetUp]
        public void SetUp()
        {
            AssemblyContext.SetupServiceLocator();
        }

        [Test]
        public void Init_with_source_id_only_should_set_source_id_and_have_no_events()
        {
            var sourceId = Guid.NewGuid();

            var sut = new CommittedEventStream(sourceId);
            sut.SourceId.Should().Be(sourceId);
            sut.Count().Should().Be(0);
        }

        [Test]
        public void Init_with_source_id_an_null_stream_should_set_source_id_and_have_no_events()
        {
            var sourceId = Guid.NewGuid();
            var nullStream = (IEnumerable<CommittedEvent>) null;

            var sut = new CommittedEventStream(sourceId, nullStream);
            sut.SourceId.Should().Be(sourceId);
            sut.Count().Should().Be(0);
        }

        [Test]
        public void Init_with_source_id_and_stream_should_set_source_id_and_contain_all_events_as_given()
        {
            var sourceId = Guid.NewGuid();
            var stream = new[]
            {
                new CommittedEvent(Guid.NewGuid(), null, Guid.NewGuid(), sourceId, 1, DateTime.Now, 0, new DummyEvent())
            };

            var sut = new CommittedEventStream(sourceId, stream);
            sut.SourceId.Should().Be(sourceId);

            sut.Should().BeEquivalentTo(stream);
        }

        [Test]
        public void Init_should_set_From_and_To_version_information()
        {
            var sourceId = Guid.NewGuid();
            var eventObjects = new[] { new DummyEvent(), new DummyEvent(), new DummyEvent() };
            var committedEvents = Prepare.Events(eventObjects).ForSource(sourceId, 5).ToList();

            var sut = new CommittedEventStream(sourceId, committedEvents);

            sut.FromVersion.Should().Be(committedEvents.First().EventSequence);
            sut.ToVersion.Should().Be(committedEvents.Last().EventSequence);
        }

        [Test]
        public void When_constructing_it_with_events_but_an_element_is_null_it_should_throw_ArgumentNullException()
        {
            var sourceId = Guid.NewGuid();
            var eventsWithAnNullElement = new[] { new CommittedEvent(Guid.NewGuid(), null, Guid.NewGuid(), sourceId, 0, DateTime.Now, 0, new DummyEvent()), null };

            Action act = () => new CommittedEventStream(sourceId, eventsWithAnNullElement);

            act.ShouldThrow<ArgumentNullException>();
        }

        [Test]
        public void When_constructing_it_with_events_where_one_has_an_incorrect_sequence_it_should_throw_argument_exception()
        {
            var sourceId = Guid.NewGuid();
            var eventObjects = new[] {new DummyEvent(), new DummyEvent(), new DummyEvent()};
            var committedEvents = Prepare.Events(eventObjects).ForSource(sourceId).ToList();

            var lastEvent = committedEvents.Last();
            const int incorrectSequence = int.MaxValue;
            var incorrectEvent = new CommittedEvent(lastEvent.CommitId, null, lastEvent.EventIdentifier, lastEvent.EventSourceId, incorrectSequence, lastEvent.EventTimeStamp, 0, lastEvent.Payload);
            committedEvents[committedEvents.Count - 1] = incorrectEvent;

            Action act = () => new CommittedEventStream(sourceId, committedEvents);

            act.ShouldThrow<ArgumentException>();
        }

        [Test]
        public void When_constructing_it_with_events_where_one_has_an_incorrect_event_source_id_it_should_throw_argument_exception()
        {
            var sourceId = Guid.NewGuid();
            var eventObjects = new[] { new DummyEvent(), new DummyEvent(), new DummyEvent() };
            var committedEvents = Prepare.Events(eventObjects).ForSource(sourceId).ToList();

            var lastEvent = committedEvents.Last();
            var incorrectSourceId = Guid.NewGuid();
            var incorrectEvent = new CommittedEvent(lastEvent.CommitId, null, lastEvent.EventIdentifier, incorrectSourceId, lastEvent.EventSequence, lastEvent.EventTimeStamp, 0, lastEvent.Payload);
            committedEvents[committedEvents.Count - 1] = incorrectEvent;

            Action act = () => new CommittedEventStream(sourceId, committedEvents);

            act.ShouldThrow<ArgumentException>();
        }
    }
}
