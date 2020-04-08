using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Ncqrs.Eventing;
using WB.Core.GenericSubdomains.Portable;
using WB.Core.GenericSubdomains.Portable.Implementation;
using WB.Core.GenericSubdomains.Portable.Services;
using WB.Core.Infrastructure.EventBus.Lite;
using WB.Core.SharedKernels.DataCollection.ValueObjects.Interview;
using WB.Core.SharedKernels.DataCollection.WebApi;
using WB.Core.SharedKernels.Enumerator.Implementation.Services.Synchronization.Steps;
using WB.Core.SharedKernels.Enumerator.Properties;
using WB.Core.SharedKernels.Enumerator.Services;
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

        private class InterviewLite
        {
            public Guid InterviewId { get; set; }
            public Guid LastHqEventId { get; set; }
        }

        public DownloadHQChangesForInterview(int sortOrder, 
            ISynchronizationService synchronizationService, 
            ILogger logger,
            IPlainStorage<InterviewView> interviewViewRepository,
            IPlainStorage<InterviewSequenceView, Guid> interviewSequenceViewRepository,
            IEnumeratorEventStorage eventStore,
            ILiteEventBus eventBus) 
            : base(sortOrder, synchronizationService, logger)
        {
            this.interviewViewRepository = interviewViewRepository;
            this.interviewSequenceViewRepository = interviewSequenceViewRepository;
            this.eventStore = eventStore;
            this.eventBus = eventBus;
        }

        public override async Task ExecuteAsync()
        {
            List<InterviewApiView> remoteInterviews = await this.synchronizationService.GetInterviewsAsync(this.Context.CancellationToken);
            var remoteInterviewWithSequence = remoteInterviews
                .Where(i => i.LastSequenceEventId.HasValue)
                .ToDictionary(k => k.Id, v => v.LastSequenceEventId.Value);

            var localInterviews = this.interviewViewRepository.LoadAll();
            var localNonCompletedInterviewIds = localInterviews
                .Where(i => i.Status != InterviewStatus.Completed)
                .Select(interview => interview.InterviewId)
                .ToDictionary(k => k, v => eventStore.GetLastEventIdUploadedToHq(v));

            var localInterviewsToUpdate = localNonCompletedInterviewIds
                .Where(interview =>
                {
                    if (!interview.Value.HasValue)
                        return false;

                    if (!remoteInterviewWithSequence.TryGetValue(interview.Key, out var lastHqEventId))
                        return false;

                    return interview.Value.Value != lastHqEventId;
                }).Select(kv => new InterviewLite()
                {
                    InterviewId = kv.Key,
                    LastHqEventId = kv.Value.Value
                }).ToList(); 

            await this.UpdateLocalInterviewsAsync(localInterviewsToUpdate, this.Context.Statistics, this.Context.Progress, this.Context.CancellationToken);
        }

        private async Task UpdateLocalInterviewsAsync(List<InterviewLite> interviews,
            SynchronizationStatistics statistics,
            IProgress<SyncProgressInfo> progress,
            CancellationToken cancellationToken)
        {
            IProgress<TransferProgress> transferProgress = progress.AsTransferReport();

            foreach (var interview in interviews)
                try
                {
                    cancellationToken.ThrowIfCancellationRequested();
                    progress.Report(new SyncProgressInfo
                    {
                        Title = EnumeratorUIResources.Synchronization_Download_Title,
                        Description = string.Format(EnumeratorUIResources.Synchronization_Download_Description_Format,
                            statistics.RejectedInterviewsCount + statistics.NewInterviewsCount + 1, interviews.Count,
                            EnumeratorUIResources.Synchronization_Interviews),
                        Stage = SyncStage.UpdatingInterviews,
                        StageExtraInfo = new Dictionary<string, string>()
                        {
                            { "processedCount", (statistics.RejectedInterviewsCount + statistics.NewInterviewsCount + 1).ToString() },
                            { "totalCount", interviews.Count.ToString()}
                        }
                    });

                    List<CommittedEvent> interviewDetails = await this.synchronizationService.GetInterviewDetailsAsyncAfterEvent(
                        interview.InterviewId, interview.LastHqEventId, transferProgress, cancellationToken);

                    if (interviewDetails == null || interviewDetails.Count == 0)
                    {
                        continue;
                    }

                    eventStore.InsertEventsFromHqInEventsStream(interview.InterviewId, new CommittedEventStream(interview.InterviewId, interviewDetails));
                    eventBus.PublishCommittedEvents(interviewDetails);

                    //await this.synchronizationService.LogInterviewAsSuccessfullyHandledAsync(interview.InterviewId);
                }
                catch (OperationCanceledException)
                {

                }
                catch (Exception exception)
                {
                    statistics.FailedToCreateInterviewsCount++;

                    await this.TrySendUnexpectedExceptionToServerAsync(exception);
                    this.logger.Error(
                        "Failed to download hq changes for interview, interviewer",
                        exception);
                }
        }
    }
}
