using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Moq;
using Ncqrs.Eventing;
using NUnit.Framework;
using WB.Core.BoundedContexts.Interviewer.Services;
using WB.Core.BoundedContexts.Interviewer.Synchronization;
using WB.Core.GenericSubdomains.Portable;
using WB.Core.GenericSubdomains.Portable.Implementation;
using WB.Core.GenericSubdomains.Portable.Services;
using WB.Core.Infrastructure.Aggregates;
using WB.Core.Infrastructure.CommandBus;
using WB.Core.Infrastructure.EventBus.Lite;
using WB.Core.SharedKernels.DataCollection.WebApi;
using WB.Core.SharedKernels.Enumerator.Implementation.Services.Synchronization.Steps;
using WB.Core.SharedKernels.Enumerator.Services;
using WB.Core.SharedKernels.Enumerator.Services.Infrastructure;
using WB.Core.SharedKernels.Enumerator.Services.Infrastructure.Storage;
using WB.Core.SharedKernels.Enumerator.Services.Synchronization;
using WB.Core.SharedKernels.Enumerator.Views;
using WB.Tests.Abc;

namespace WB.Tests.Unit.BoundedContexts.Interviewer.SynchronizationSteeps
{
    [TestFixture]
    public class DownloadHqChangesForInterviewTests
    {
        [Test]
        public async Task when_hq_contains_new_event_for_interview()
        {
            var interviewId = Id.g1;
            var lastLocalEventId = Id.g2;
            var newEventIdFromHq = Id.g3;

            List<InterviewApiView> remoteInterviews = new List<InterviewApiView>()
            {
                Create.Entity.InterviewApiView(interviewId, newEventIdFromHq),
            };

            List<InterviewView> localInterviews = new List<InterviewView>()
            {
                Create.Entity.InterviewView(interviewId: interviewId)
            };

            List<EventsAfter> eventsAfter = new List<EventsAfter>()
            {
                CreateEventsAfter(interviewId, lastLocalEventId, Create.Event.CommittedEvent(eventSourceId: interviewId, eventIdentifier: newEventIdFromHq)),
            };

            var eventStore = Mock.Of<IEnumeratorEventStorage>(s => s.GetLastEventIdUploadedToHq(interviewId) == lastLocalEventId);
            var eventBus = Mock.Of<ILiteEventBus>();

            var synchronizationStep = CreateDownloadHQChangesForInterview(remoteInterviews, localInterviews, eventsAfter,
                eventStore, eventBus);

            await synchronizationStep.ExecuteAsync();

            Mock.Get(eventStore).Verify(e =>
                e.InsertEventsFromHqInEventsStream(interviewId, It.Is<CommittedEventStream>(s => s.Count == 1 && s.Single().EventIdentifier == newEventIdFromHq)), Times.Once);
            Mock.Get(eventBus).Verify(e =>
                e.PublishCommittedEvents(It.Is<IReadOnlyCollection<CommittedEvent>>(s => s.Count == 1 && s.Single().EventIdentifier == newEventIdFromHq)), Times.Once);
        }

        [Test]
        public async Task when_hq_dont_contains_data_about_interview_created_on_client()
        {
            var interviewId = Id.g1;
            var lastLocalEventId = Id.g2;

            List<InterviewApiView> remoteInterviews = new List<InterviewApiView>()
            {
            };

            List<InterviewView> localInterviews = new List<InterviewView>()
            {
                Create.Entity.InterviewView(interviewId: interviewId)
            };

            List<EventsAfter> eventsAfter = new List<EventsAfter>()
            {
            };

            var eventStore = Mock.Of<IEnumeratorEventStorage>(s => s.GetLastEventIdUploadedToHq(interviewId) == lastLocalEventId);

            var synchronizationStep = CreateDownloadHQChangesForInterview(remoteInterviews, localInterviews, eventsAfter,
                eventStore);

            await synchronizationStep.ExecuteAsync();

            Mock.Get(eventStore).Verify(e =>
                e.InsertEventsFromHqInEventsStream(interviewId, It.IsAny<CommittedEventStream>()), Times.Never);
        }

        [Test]
        public async Task when_hq_contains_the_same_data_about_interview()
        {
            var interviewId = Id.g1;
            var lastLocalEventId = Id.g2;

            List<InterviewApiView> remoteInterviews = new List<InterviewApiView>()
            {
                Create.Entity.InterviewApiView(interviewId, lastLocalEventId),
            };

            List<InterviewView> localInterviews = new List<InterviewView>()
            {
                Create.Entity.InterviewView(interviewId: interviewId)
            };

            List<EventsAfter> eventsAfter = new List<EventsAfter>()
            {
                CreateEventsAfter(interviewId, lastLocalEventId),
            };

            var eventStore = Mock.Of<IEnumeratorEventStorage>(s => s.GetLastEventIdUploadedToHq(interviewId) == lastLocalEventId);
            var eventBus = Mock.Of<ILiteEventBus>();

            var synchronizationStep = CreateDownloadHQChangesForInterview(remoteInterviews, localInterviews, eventsAfter,
                eventStore, eventBus);

            await synchronizationStep.ExecuteAsync();

            Mock.Get(eventStore).Verify(e =>
                e.InsertEventsFromHqInEventsStream(interviewId, It.IsAny<CommittedEventStream>()), Times.Never);
            Mock.Get(eventBus).Verify(e =>
                e.PublishCommittedEvents(It.IsAny<IReadOnlyCollection<CommittedEvent>>()), Times.Never);
        }

        [Test]
        public async Task when_hq_contains_data_about_interview_but_localy_it_doesnot_exists()
        {
            var interviewId = Id.g1;

            List<InterviewApiView> remoteInterviews = new List<InterviewApiView>()
            {
                Create.Entity.InterviewApiView(interviewId, Id.g10),
            };

            List<InterviewView> localInterviews = new List<InterviewView>()
            {
            };

            List<EventsAfter> eventsAfter = new List<EventsAfter>()
            {
            };

            var eventStore = Mock.Of<IEnumeratorEventStorage>();
            var eventBus = Mock.Of<ILiteEventBus>();

            var synchronizationStep = CreateDownloadHQChangesForInterview(remoteInterviews, localInterviews, eventsAfter,
                eventStore, eventBus);

            await synchronizationStep.ExecuteAsync();

            Mock.Get(eventStore).Verify(e =>
                e.InsertEventsFromHqInEventsStream(interviewId, It.IsAny<CommittedEventStream>()), Times.Never);
            Mock.Get(eventBus).Verify(e =>
                e.PublishCommittedEvents(It.IsAny<IReadOnlyCollection<CommittedEvent>>()), Times.Never);
        }

        private SynchronizationStep CreateDownloadHQChangesForInterview(
            List<InterviewApiView> remoteInterviews,
            List<InterviewView> localInterviews,
            List<EventsAfter> eventsAfter,
            IEnumeratorEventStorage eventStore = null,
            ILiteEventBus eventBus = null)
        {
            var synchronizationService = new Mock<ISynchronizationService>();
            synchronizationService.Setup(s => s.GetInterviewsAsync(It.IsAny<CancellationToken>())).Returns(Task.FromResult(remoteInterviews));

            foreach (var after in eventsAfter)
            {
                synchronizationService
                    .Setup(s => s.GetInterviewDetailsAfterEventAsync(after.InterviewId, after.EventId, It.IsAny<IProgress<TransferProgress>>(), It.IsAny<CancellationToken>()))
                    .Returns(Task.FromResult(after.Events));
            }

            var localStorage = Mock.Of<IPlainStorage<InterviewView>>(s =>
                s.LoadAll() == localInterviews.ToReadOnlyCollection());

            return CreateDownloadHQChangesForInterview(synchronizationService.Object,
                localStorage,
                eventStore: eventStore,
                eventBus: eventBus);
        }

        private SynchronizationStep CreateDownloadHQChangesForInterview(
            ISynchronizationService synchronizationService = null,
            IPlainStorage<InterviewView> interviewViewRepository = null,
            IPlainStorage<InterviewSequenceView, Guid> interviewSequenceViewRepository = null,
            IEnumeratorEventStorage eventStore = null,
            ILiteEventBus eventBus = null,
            ICommandService commandService =null,
            IPrincipal principal = null,
            IEventSourcedAggregateRootRepositoryCacheCleaner aggregateRootRepositoryCacheCleaner = null,
            IInterviewerSettings interviewerSettings = null)
        {
            var downloadHqChangesForInterview = new DownloadHQChangesForInterview(0,
                synchronizationService ?? Mock.Of<ISynchronizationService>(),
                Mock.Of<ILogger>(),
                interviewViewRepository ?? Mock.Of<IPlainStorage<InterviewView>>(),
                interviewSequenceViewRepository ?? Mock.Of<IPlainStorage<InterviewSequenceView, Guid>>(),
                eventStore ?? Mock.Of<IEnumeratorEventStorage>(),
                eventBus ?? Mock.Of<ILiteEventBus>(),
                commandService ?? Mock.Of<ICommandService>(),
                principal ?? Mock.Of<IPrincipal>(),
                aggregateRootRepositoryCacheCleaner ?? Mock.Of<IEventSourcedAggregateRootRepositoryCacheCleaner>(),
                interviewerSettings ?? Mock.Of<IInterviewerSettings>(s => s.PartialSynchronizationEnabled == true && s.AllowSyncWithHq == true));

            downloadHqChangesForInterview.Context = new EnumeratorSynchonizationContext();
            downloadHqChangesForInterview.Context.Progress = new Progress<SyncProgressInfo>();
            downloadHqChangesForInterview.Context.Statistics = new SynchronizationStatistics();

            return downloadHqChangesForInterview;
        }

        private EventsAfter CreateEventsAfter(Guid interviewId, Guid eventId, params CommittedEvent[] events)
        {
            return new EventsAfter()
            {
                InterviewId = interviewId,
                EventId = eventId,
                Events = events.ToList()
            };
        }

        private class EventsAfter
        {
            public Guid InterviewId { get; set; }
            public Guid EventId { get; set; }
            public List<CommittedEvent> Events { get; set; }
        }
    }
}
