using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Ncqrs.Eventing;
using WB.Core.BoundedContexts.Interviewer.Services;
using WB.Core.BoundedContexts.Interviewer.Services.Infrastructure;
using WB.Core.BoundedContexts.Interviewer.Views;
using WB.Core.GenericSubdomains.Portable;
using WB.Core.GenericSubdomains.Portable.Implementation;
using WB.Core.GenericSubdomains.Portable.Services;
using WB.Core.Infrastructure.Aggregates;
using WB.Core.Infrastructure.CommandBus;
using WB.Core.Infrastructure.EventBus.Lite;
using WB.Core.SharedKernels.DataCollection.Commands.Interview;
using WB.Core.SharedKernels.DataCollection.Events.Interview;
using WB.Core.SharedKernels.DataCollection.Events.Interview.Base;
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
        private readonly IEnumeratorEventStorage eventStore;
        private readonly ILiteEventBus eventBus;
        private readonly ICommandService commandService;
        private readonly IInterviewerPrincipal principal;
        private readonly IEventSourcedAggregateRootRepositoryCacheCleaner aggregateRootRepositoryCacheCleaner;
        private readonly IInterviewerSettings interviewerSettings;

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
            IEnumeratorEventStorage eventStore,
            ILiteEventBus eventBus,
            ICommandService commandService,
            IInterviewerPrincipal principal,
            IEventSourcedAggregateRootRepositoryCacheCleaner aggregateRootRepositoryCacheCleaner,
            IInterviewerSettings interviewerSettings) 
            : base(sortOrder, synchronizationService, logger)
        {
            this.interviewViewRepository = interviewViewRepository;
            this.eventStore = eventStore;
            this.eventBus = eventBus;
            this.commandService = commandService;
            this.principal = principal;
            this.aggregateRootRepositoryCacheCleaner = aggregateRootRepositoryCacheCleaner;
            this.interviewerSettings = interviewerSettings;
        }

        public override async Task ExecuteAsync()
        {
            if (!interviewerSettings.PartialSynchronizationEnabled || !interviewerSettings.AllowSyncWithHq)
                return;

            List<InterviewApiView> remoteInterviews = await this.synchronizationService.GetInterviewsAsync(this.Context.CancellationToken);
            var remoteInterviewWithSequence = remoteInterviews
                .Where(i => i.LastEventId.HasValue)
                .ToDictionary(k => k.Id, v => v.LastEventId.GetValueOrDefault());

            var localInterviews = this.interviewViewRepository.LoadAll();
            var localPartialySyncedInterviews = localInterviews
                .Select(i => new
                {
                    InterviewId = i.InterviewId,
                    LastHqEventId = eventStore.GetLastEventIdUploadedToHq(i.InterviewId),
                    IsCompleted = i.Status == InterviewStatus.Completed
                })
                .Where(i => i.LastHqEventId.HasValue)
                .ToDictionary(k => k.InterviewId, v => new InterviewLite {
                    InterviewId = v.InterviewId,
                    LastHqEventId = v.LastHqEventId.GetValueOrDefault(),
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
            {
                var interview = interviews[i];

                try
                {
                    cancellationToken.ThrowIfCancellationRequested();
                    progress.Report(new SyncProgressInfo
                    {
                        Title = EnumeratorUIResources.Synchronization_Update_Interviews_Title,
                        Description = string.Format(EnumeratorUIResources.Synchronization_Download_Description_Format,
                            i + 1, interviews.Count, EnumeratorUIResources.Synchronization_PartialInterviews),
                        Stage = SyncStage.UpdatingInterviewsChanges,
                        StageExtraInfo = new Dictionary<string, string>()
                        {
                            { "processedCount", (i + 1).ToString() },
                            { "totalCount", interviews.Count.ToString()}
                        }
                    });

                    List<CommittedEvent> events = await this.synchronizationService.GetInterviewDetailsAfterEventAsync(
                        interview.InterviewId, interview.LastHqEventId, transferProgress, cancellationToken);

                    if (events == null || events.Count == 0)
                    {
                        continue;
                    }

                    if (!IsCanInsertEventsInStream(events))
                    {
                        statistics.FailToPartialProcessInterviewIds.Add(interview.InterviewId);
                        statistics.FailedToPartialDownloadedInterviewsCount++;
                        continue;
                    }

                    eventStore.InsertEventsFromHqInEventsStream(interview.InterviewId, new CommittedEventStream(interview.InterviewId, events));
                    eventBus.PublishCommittedEvents(events);

                    aggregateRootRepositoryCacheCleaner.CleanCache();


                    if (interview.IsCompleted && DoNewEventsHaveComments(events))
                    {
                        var supervisorId = ((InterviewerIdentity)principal.CurrentUserIdentity).SupervisorId;
                        var command = new RestartInterviewCommand(interview.InterviewId, supervisorId, "[system: Reopened after getting new comments]", DateTime.Now);
                        commandService.Execute(command);
                        statistics.ReopenedInterviewsAfterReceivedCommentsCount++;
                    }

                    statistics.SuccessfullyPartialDownloadedInterviewsCount++;
                }
                catch (OperationCanceledException)
                {
                    throw;
                }
                catch (Exception exception)
                {
                    statistics.FailedToPartialDownloadedInterviewsCount++;

                    await this.TrySendUnexpectedExceptionToServerAsync(exception);

                    var name = principal.CurrentUserIdentity.Name;
                    this.logger.Error(
                        $"Failed to partial download hq changes for interview {interview.InterviewId}, interviewer {name}",
                        exception);
                }
            }
        }

        private bool DoNewEventsHaveComments(List<CommittedEvent> events)
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
                        var isAnswerEvent = @event.Payload.GetType().IsSubclassOf(typeof(QuestionAnswered));
                        return !isAnswerEvent;
                }
            });
        }
    }
}
