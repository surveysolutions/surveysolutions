using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Ncqrs.Eventing;
using Ncqrs.Eventing.ServiceModel.Bus;
using WB.Core.GenericSubdomains.Portable;
using WB.Core.GenericSubdomains.Portable.Implementation;
using WB.Core.GenericSubdomains.Portable.Services;
using WB.Core.Infrastructure.EventBus.Lite;
using WB.Core.SharedKernels.DataCollection.WebApi;
using WB.Core.SharedKernels.Enumerator.Properties;
using WB.Core.SharedKernels.Enumerator.Services;
using WB.Core.SharedKernels.Enumerator.Services.Infrastructure.Storage;
using WB.Core.SharedKernels.Enumerator.Services.Synchronization;
using WB.Core.SharedKernels.Enumerator.Views;

namespace WB.Core.SharedKernels.Enumerator.Implementation.Services.Synchronization.Steps
{
    public abstract class DownloadInterviews : SynchronizationStep
    {
        protected readonly ISynchronizationService SynchronizationService;
        private readonly IQuestionnaireDownloader questionnaireDownloader;
        private readonly IPlainStorage<InterviewSequenceView, Guid> interviewSequenceViewRepository;
        private readonly IPlainStorage<InterviewView> interviewViewRepository;
        private readonly ILiteEventBus eventBus;
        protected readonly IEnumeratorEventStorage EventStore;
        private readonly ILogger logger;
        private readonly IInterviewsRemover interviewsRemover;

        protected DownloadInterviews(ISynchronizationService synchronizationService,
            IQuestionnaireDownloader questionnaireDownloader, 
            IPlainStorage<InterviewSequenceView, Guid> interviewSequenceViewRepository, 
            IPlainStorage<InterviewView> interviewViewRepository, 
            ILiteEventBus eventBus, 
            IEnumeratorEventStorage eventStore, 
            ILogger logger, 
            IInterviewsRemover interviewsRemover, 
            int sortOder) : base(sortOder, synchronizationService, logger)
        {
            this.SynchronizationService = synchronizationService ?? throw new ArgumentNullException(nameof(synchronizationService));
            this.questionnaireDownloader = questionnaireDownloader ?? throw new ArgumentNullException(nameof(questionnaireDownloader));
            this.interviewSequenceViewRepository = interviewSequenceViewRepository ?? throw new ArgumentNullException(nameof(interviewSequenceViewRepository));
            this.interviewViewRepository = interviewViewRepository ?? throw new ArgumentNullException(nameof(interviewViewRepository));
            this.eventBus = eventBus ?? throw new ArgumentNullException(nameof(eventBus));
            this.EventStore = eventStore ?? throw new ArgumentNullException(nameof(eventStore));
            this.logger = logger ?? throw new ArgumentNullException(nameof(logger));
            this.interviewsRemover = interviewsRemover ?? throw new ArgumentNullException(nameof(interviewsRemover));
        }

        public override async Task ExecuteAsync()
        {
            List<InterviewApiView> remoteInterviews = await this.SynchronizationService.GetInterviewsAsync(this.Context.CancellationToken);
            var remoteInterviewWithSequence = remoteInterviews.ToDictionary(k => k.Id, v => v.Sequence);

            var localInterviews = this.interviewViewRepository.LoadAll();
            var localInterviewIds = localInterviews.Select(interview => interview.InterviewId).ToHashSet();

            var localInterviewsToRemove = localInterviews.Where(
                interview => !remoteInterviewWithSequence.ContainsKey(interview.InterviewId) && !interview.CanBeDeleted);

            var obsoleteInterviews = await this.FindObsoleteInterviewsAsync(localInterviews, remoteInterviews, this.Context.Progress, this.Context.CancellationToken);

            var localInterviewIdsToRemove = localInterviewsToRemove
                .Select(interview => interview.InterviewId)
                .Where(IsNotPresentOnHq)
                .Concat(obsoleteInterviews)
                .ToArray();

            var remoteInterviewsToCreate = remoteInterviews
                .Where(interview => (!localInterviewIds.Contains(interview.Id) && ShouldBeDownloadedBasedOnEventSequence(interview)) || obsoleteInterviews.Contains(interview.Id))
                .ToList();

            this.interviewsRemover.RemoveInterviews(this.Context.Statistics, this.Context.Progress, localInterviewIdsToRemove);

            var interviewsThatAreNotMarkedAsReceived = remoteInterviews.Where(x => !x.IsMarkedAsReceivedByInterviewer && 
                                                                                   remoteInterviewsToCreate.All(r => r.Id != x.Id));
            foreach (var interviewApiView in interviewsThatAreNotMarkedAsReceived)
            {
                this.Context.CancellationToken.ThrowIfCancellationRequested();
                await this.SynchronizationService.LogInterviewAsSuccessfullyHandledAsync(interviewApiView.Id);
            }

            await this.CreateInterviewsAsync(remoteInterviewsToCreate, this.Context.Statistics, this.Context.Progress, this.Context.CancellationToken);
        }

        private async Task CreateInterviewsAsync(List<InterviewApiView> interviews,
            SynchronizationStatistics statistics,
            IProgress<SyncProgressInfo> progress,
            CancellationToken cancellationToken)
        {
            statistics.TotalNewInterviewsCount = interviews.Count(interview => !interview.IsRejected);
            statistics.TotalRejectedInterviewsCount = interviews.Count(interview => interview.IsRejected);

            IProgress<TransferProgress> transferProgress = progress.AsTransferReport();

            foreach (var interview in interviews)
                try
                {
                    cancellationToken.ThrowIfCancellationRequested();
                    progress.Report(new SyncProgressInfo
                    {
                        Title = InterviewerUIResources.Synchronization_Download_Title,
                        Description = string.Format(InterviewerUIResources.Synchronization_Download_Description_Format,
                            statistics.RejectedInterviewsCount + statistics.NewInterviewsCount + 1, interviews.Count,
                            InterviewerUIResources.Synchronization_Interviews),
                        Stage = SyncStage.UpdatingAssignments
                    });

                    await this.questionnaireDownloader.DownloadQuestionnaireAsync(interview.QuestionnaireIdentity, statistics, transferProgress, cancellationToken);

                    List<CommittedEvent> interviewDetails = await this.SynchronizationService.GetInterviewDetailsAsync(
                        interview.Id, transferProgress, cancellationToken);

                    if (interviewDetails == null)
                    {
                        statistics.NewInterviewsCount++;
                        continue;
                    }

                    eventBus.PublishCommittedEvents(interviewDetails);
                    EventStore.StoreEvents(new CommittedEventStream(interview.Id, interviewDetails));
                    MarkInterviewAsReceivedFromHeadquarters(interview);

                    if (interview.Sequence.HasValue)
                        interviewSequenceViewRepository.Store(new InterviewSequenceView() { Id = interview.Id, ReceivedFromServerSequence = interview.Sequence.Value });

                    await this.SynchronizationService.LogInterviewAsSuccessfullyHandledAsync(interview.Id);

                    if (interview.IsRejected)
                        statistics.RejectedInterviewsCount++;
                    else
                        statistics.NewInterviewsCount++;
                }
                catch (OperationCanceledException)
                {

                }
                catch (Exception exception)
                {
                    statistics.FailedToCreateInterviewsCount++;

                    await this.TrySendUnexpectedExceptionToServerAsync(exception);
                    this.logger.Error(
                        "Failed to create interview, interviewer",
                        exception);
                }
        }

        
        private bool IsNotPresentOnHq(Guid interviewId)
        {
            return interviewSequenceViewRepository.GetById(interviewId) != null;
        }

        private bool ShouldBeDownloadedBasedOnEventSequence(InterviewApiView interview)
        {
            if (!interview.Sequence.HasValue)
                return true;
            var interviewSequenceView = interviewSequenceViewRepository.GetById(interview.Id);
            if (interviewSequenceView == null)
                return true;
            return interviewSequenceView.ReceivedFromServerSequence < interview.Sequence;
        }

        private void MarkInterviewAsReceivedFromHeadquarters(InterviewApiView interview)
        {
            var dashboardItem = this.interviewViewRepository.GetById(interview.Id.FormatGuid());
            dashboardItem.CanBeDeleted = false;
            dashboardItem.FromHqSyncDateTime = DateTime.UtcNow;
            this.interviewViewRepository.Store(dashboardItem);
        }

        protected abstract Task<List<Guid>> FindObsoleteInterviewsAsync(IEnumerable<InterviewView> localInterviews,
            IEnumerable<InterviewApiView> remoteInterviews,
            IProgress<SyncProgressInfo> progress,
            CancellationToken cancellationToken);
    }
}
