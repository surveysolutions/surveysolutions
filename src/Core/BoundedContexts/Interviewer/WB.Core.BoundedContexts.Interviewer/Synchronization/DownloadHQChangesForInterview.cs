using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Ncqrs.Eventing;
using WB.Core.GenericSubdomains.Portable;
using WB.Core.GenericSubdomains.Portable.Implementation;
using WB.Core.GenericSubdomains.Portable.Services;
using WB.Core.Infrastructure.Aggregates;
using WB.Core.Infrastructure.CommandBus;
using WB.Core.Infrastructure.EventBus.Lite;
using WB.Core.SharedKernels.DataCollection.Commands.Interview;
using WB.Core.SharedKernels.DataCollection.Events.Interview;
using WB.Core.SharedKernels.DataCollection.ValueObjects.Interview;
using WB.Core.SharedKernels.DataCollection.WebApi;
using WB.Core.SharedKernels.Enumerator.Implementation.Services.Synchronization.Steps;
using WB.Core.SharedKernels.Enumerator.Properties;
using WB.Core.SharedKernels.Enumerator.Services;
using WB.Core.SharedKernels.Enumerator.Services.Infrastructure;
using WB.Core.SharedKernels.Enumerator.Services.Infrastructure.Storage;
using WB.Core.SharedKernels.Enumerator.Services.Synchronization;
using WB.Core.SharedKernels.Enumerator.Views;

namespace WB.Core.BoundedContexts.Interviewer.Synchronization
{
    public class DownloadHQChangesForInterview : SynchronizationStep
    {
        private readonly IPlainStorage<InterviewView> interviewViewRepository;
        private readonly IPlainStorage<InterviewSequenceView, Guid> interviewSequenceViewRepository;
        private readonly IEnumeratorEventStorage eventStore;
        private readonly ILiteEventBus eventBus;
        private readonly ICommandService commandService;
        private readonly IPrincipal principal;
        private readonly IEventSourcedAggregateRootRepositoryCacheCleaner aggregateRootRepositoryCacheCleaner;

        private class InterviewLite
        {
            public Guid InterviewId { get; set; }
            public Guid LastHqEventId { get; set; }
            public bool IsCompleted { get; set; }
        }

        public DownloadHQChangesForInterview(int sortOrder, 
            ISynchronizationService synchronizationService, 
            ILogger logger,
            IPlainStorage<InterviewView> interviewViewRepository,
            IPlainStorage<InterviewSequenceView, Guid> interviewSequenceViewRepository,
            IEnumeratorEventStorage eventStore,
            ILiteEventBus eventBus,
            ICommandService commandService,
            IPrincipal principal,
            IEventSourcedAggregateRootRepositoryCacheCleaner aggregateRootRepositoryCacheCleaner) 
            : base(sortOrder, synchronizationService, logger)
        {
            this.interviewViewRepository = interviewViewRepository;
            this.interviewSequenceViewRepository = interviewSequenceViewRepository;
            this.eventStore = eventStore;
            this.eventBus = eventBus;
            this.commandService = commandService;
            this.principal = principal;
            this.aggregateRootRepositoryCacheCleaner = aggregateRootRepositoryCacheCleaner;
        }

        public override async Task ExecuteAsync()
        {
            List<InterviewApiView> remoteInterviews = await this.synchronizationService.GetInterviewsAsync(this.Context.CancellationToken);
            var remoteInterviewWithSequence = remoteInterviews
                .Where(i => i.LastSequenceEventId.HasValue)
                .ToDictionary(k => k.Id, v => v.LastSequenceEventId.Value);

            var localInterviews = this.interviewViewRepository.LoadAll();
            var localPartialySyncedInterviews = localInterviews
                .Select(i => new
                {
                    InterviewId = i.InterviewId,
                    LastHqEventId = eventStore.GetLastEventIdUploadedToHq(i.InterviewId),
                    IsCompleted = i.Status == InterviewStatus.Completed
                })
                .Where(i => i.LastHqEventId.HasValue)
                .ToDictionary(k => k.InterviewId, v => new InterviewLite(){
                    InterviewId = v.InterviewId,
                    LastHqEventId = v.LastHqEventId.Value,
                    IsCompleted = v.IsCompleted
                });

            var localInterviewsToUpdate = localPartialySyncedInterviews
                .Where(interview =>
                {
                    if (!remoteInterviewWithSequence.TryGetValue(interview.Key, out var lastHqEventId))
                        return false;

                    return interview.Value.LastHqEventId != lastHqEventId;
                }).Select(kv => kv.Value).ToList(); 

            await this.UpdateLocalInterviewsAsync(localInterviewsToUpdate, this.Context.Statistics, this.Context.Progress, this.Context.CancellationToken);
        }

        private async Task UpdateLocalInterviewsAsync(List<InterviewLite> interviews,
            SynchronizationStatistics statistics,
            IProgress<SyncProgressInfo> progress,
            CancellationToken cancellationToken)
        {
            IProgress<TransferProgress> transferProgress = progress.AsTransferReport();

            for (int i = 0; i < interviews.Count; i++)
                try
                {
                    var interview = interviews[i];
                    cancellationToken.ThrowIfCancellationRequested();
                    progress.Report(new SyncProgressInfo
                    {
                        Title = EnumeratorUIResources.Synchronization_Update_Interviews_Title,
                        Description = string.Format(EnumeratorUIResources.Synchronization_Download_Description_Format,
                            i + 1, interviews.Count,
                            EnumeratorUIResources.Synchronization_Interviews),
                        Stage = SyncStage.UpdatingInterviewsChanges,
                        StageExtraInfo = new Dictionary<string, string>()
                        {
                            { "processedCount", (i + 1).ToString() },
                            { "totalCount", interviews.Count.ToString()}
                        }
                    });

                    List<CommittedEvent> interviewDetails = await this.synchronizationService.GetInterviewDetailsAsyncAfterEvent(
                        interview.InterviewId, interview.LastHqEventId, transferProgress, cancellationToken);

                    if (interviewDetails == null || interviewDetails.Count == 0)
                    {
                        continue;
                    }

                    if (!IsCanInsertEventsInStream(interviewDetails))
                    {
                        continue;
                    }

                    eventStore.InsertEventsFromHqInEventsStream(interview.InterviewId, new CommittedEventStream(interview.InterviewId, interviewDetails));
                    eventBus.PublishCommittedEvents(interviewDetails);

                    aggregateRootRepositoryCacheCleaner.CleanCache();


                    if (DoesNewEventsHaveComments(interviewDetails))
                    {
                        var userId = principal.CurrentUserIdentity.UserId;
                        var command = new RestartInterviewCommand(interview.InterviewId, userId, "reopen after get new comments", DateTime.Now);
                        commandService.Execute(command);
                        statistics.ReopenedInterviewsAfterReceivedCommentsCount++;
                    }

                    statistics.SuccessfullyDownloadedPatchesForInterviewsCount++;
                }
                catch (OperationCanceledException)
                {

                }
                catch (Exception exception)
                {
                    await this.TrySendUnexpectedExceptionToServerAsync(exception);
                    this.logger.Error(
                        "Failed to download hq changes for interview, interviewer",
                        exception);
                }
        }

        private bool DoesNewEventsHaveComments(List<CommittedEvent> events)
        {
            return events.Any(@event =>
            {
                switch (@event.Payload)
                {
                    case AnswerCommented answerCommented:
                        return true;

                    default:
                        return false;
                }
            });
        }

        private bool IsCanInsertEventsInStream(List<CommittedEvent> events)
        {
            return events.All(@event =>
            {
                switch (@event.Payload)
                {
                    case InterviewReceivedByInterviewer interviewReceivedByInterviewer:
                    case InterviewReceivedBySupervisor interviewReceivedBySupervisor:
                    case AnswerCommented answerCommented:
                    case AnswerCommentResolved answerCommentResolved:
                    case InterviewPaused interviewPaused:
                    case InterviewResumed interviewResumed:
                    case InterviewKeyAssigned interviewKeyAssigned:
                        return true;

                    default:
                        return false;
                }
            });
        }
    }
}
