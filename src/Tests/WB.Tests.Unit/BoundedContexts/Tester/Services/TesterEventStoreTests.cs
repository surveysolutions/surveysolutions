using System;
using System.Linq;
using FluentAssertions;
using Ncqrs.Eventing;
using NUnit.Framework;
using WB.Core.BoundedContexts.Tester.Implementation.Services;
using WB.Core.SharedKernels.DataCollection.Implementation.Entities;
using WB.Tests.Abc;
using IEvent = WB.Core.Infrastructure.EventBus.IEvent;

namespace WB.Tests.Unit.BoundedContexts.Tester.Services
{
    [TestFixture]
    [TestOf(typeof(TesterEventStore))]
    public class TesterEventStoreTests
    {
        private class TestEvent : IEvent { }

        [Test]
        public void when_aggregate_cache_is_evicted_should_preserve_interview_events()
        {
            var store = new TesterEventStore();
            var cache = Create.Storage.NewAggregateRootCache();
            var interviewId = Guid.NewGuid();
            var stream = new UncommittedEventStream(Guid.NewGuid(), "tester");
            stream.Append(new UncommittedEvent(Guid.NewGuid(), interviewId, 1, 0, DateTime.UtcNow, new TestEvent()));
            stream.Append(new UncommittedEvent(Guid.NewGuid(), interviewId, 2, 0, DateTime.UtcNow, new TestEvent()));
            store.Store(stream);

            cache.CreateEntry(interviewId);
            cache.Evict(interviewId);
            cache.Clear();

            store.Read(interviewId, 0).Select(e => e.EventSequence).Should().Equal(1, 2);
            store.GetLastEventSequence(interviewId).Should().Be(2);
            store.Read(interviewId, 2).Select(e => e.EventSequence).Should().Equal(2);
        }

        [Test]
        public void when_events_are_appended_should_not_change_an_existing_read()
        {
            var store = new TesterEventStore();
            var interviewId = Guid.NewGuid();
            var first = new UncommittedEventStream(Guid.NewGuid(), null);
            first.Append(new UncommittedEvent(Guid.NewGuid(), interviewId, 1, 0, DateTime.UtcNow, new TestEvent()));
            store.Store(first);
            var snapshot = store.Read(interviewId, 0);

            var second = new UncommittedEventStream(Guid.NewGuid(), null);
            second.Append(new UncommittedEvent(Guid.NewGuid(), interviewId, 2, 1, DateTime.UtcNow, new TestEvent()));
            store.Store(second);

            snapshot.Select(e => e.EventSequence).Should().Equal(1);
            store.Read(interviewId, 0).Select(e => e.EventSequence).Should().Equal(1, 2);
        }

        [Test]
        public void when_retiring_an_interview_should_keep_questionnaire_revision_until_its_last_history_is_retired()
        {
            var store = new TesterEventStore();
            var questionnaireId = Guid.NewGuid();
            var firstInterviewId = Guid.NewGuid();
            var secondInterviewId = Guid.NewGuid();
            var firstStream = new UncommittedEventStream(Guid.NewGuid(), null);
            firstStream.Append(new UncommittedEvent(Guid.NewGuid(), firstInterviewId, 1, 0, DateTime.UtcNow,
                Create.Event.InterviewCreated(questionnaireId, 1)));
            var secondStream = new UncommittedEventStream(Guid.NewGuid(), null);
            secondStream.Append(new UncommittedEvent(Guid.NewGuid(), secondInterviewId, 1, 0, DateTime.UtcNow,
                Create.Event.InterviewCreated(questionnaireId, 1)));
            store.Store(firstStream);
            store.Store(secondStream);

            store.RetireInterview(firstInterviewId).Should().BeNull();
            store.Read(firstInterviewId, 0).Should().BeEmpty();

            store.RetireInterview(secondInterviewId)
                .Should().Be(new QuestionnaireIdentity(questionnaireId, 1));
            store.Read(secondInterviewId, 0).Should().BeEmpty();
        }
    }
}
