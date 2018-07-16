using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Humanizer;
using Ncqrs.Eventing;
using WB.Core.GenericSubdomains.Portable;
using WB.Core.GenericSubdomains.Portable.Implementation;
using WB.Core.GenericSubdomains.Portable.Services;
using WB.Core.Infrastructure.EventBus.Lite;
using WB.Core.SharedKernels.DataCollection.Repositories;
using WB.Core.SharedKernels.DataCollection.ValueObjects.Interview;
using WB.Core.SharedKernels.DataCollection.WebApi;
using WB.Core.SharedKernels.Enumerator.Properties;
using WB.Core.SharedKernels.Enumerator.Services;
using WB.Core.SharedKernels.Enumerator.Services.Infrastructure;
using WB.Core.SharedKernels.Enumerator.Services.Infrastructure.Storage;
using WB.Core.SharedKernels.Enumerator.Services.Synchronization;
using WB.Core.SharedKernels.Enumerator.Views;

namespace WB.Core.SharedKernels.Enumerator.Implementation.Services.Synchronization
{
    public abstract class SynchronizationProcessBase : AbstractSynchronizationProcess, ISynchronizationProcess
    {
        private readonly AttachmentsCleanupService cleanupService;
        private readonly IHttpStatistician httpStatistician;
        private readonly IAssignmentDocumentsStorage assignmentsStorage;
        private readonly IInterviewerInterviewAccessor interviewFactory;
        private readonly IAudioFileStorage audioFileStorage;
        private readonly ITabletDiagnosticService diagnosticService;
        protected readonly IAuditLogSynchronizer auditLogSynchronizer;
        private readonly ILiteEventBus eventBus;
        private readonly IEnumeratorEventStorage eventStore;
        private readonly IPlainStorage<InterviewFileView> imagesStorage;
        private readonly IPlainStorage<InterviewMultimediaView> interviewMultimediaViewStorage;
        private readonly IPlainStorage<InterviewView> interviewViewRepository;
        private readonly ILogger logger;
        protected readonly CompanyLogoSynchronizer logoSynchronizer;
        protected readonly IAssignmentsSynchronizer assignmentsSynchronizer;
        private readonly IQuestionnaireDownloader questionnaireDownloader;
        private readonly IPrincipal principal;
        private readonly IInterviewerQuestionnaireAccessor questionnairesAccessor;
        protected readonly ISynchronizationService synchronizationService;

        protected SynchronizationProcessBase(ISynchronizationService synchronizationService,
            IPlainStorage<InterviewView> interviewViewRepository,
            IPrincipal principal,
            ILogger logger,
            IUserInteractionService userInteractionService,
            IInterviewerQuestionnaireAccessor questionnairesAccessor,
            IInterviewerInterviewAccessor interviewFactory,
            IPlainStorage<InterviewMultimediaView> interviewMultimediaViewStorage,
            IPlainStorage<InterviewFileView> imagesStorage,
            CompanyLogoSynchronizer logoSynchronizer,
            AttachmentsCleanupService cleanupService,
            IPasswordHasher passwordHasher,
            IAssignmentsSynchronizer assignmentsSynchronizer,
            IQuestionnaireDownloader questionnaireDownloader,
            IHttpStatistician httpStatistician,
            IAssignmentDocumentsStorage assignmentsStorage,
            IAudioFileStorage audioFileStorage,
            ITabletDiagnosticService diagnosticService,
            IAuditLogSynchronizer auditLogSynchronizer,
            IAuditLogService auditLogService,
            ILiteEventBus eventBus,
            IEnumeratorEventStorage eventStore) : base(synchronizationService, logger,
            httpStatistician, userInteractionService, principal, passwordHasher, 
            interviewViewRepository, auditLogService)
        {
            this.synchronizationService = synchronizationService;
            this.interviewViewRepository = interviewViewRepository;
            this.principal = principal;
            this.logger = logger;
            this.questionnairesAccessor = questionnairesAccessor;
            this.interviewFactory = interviewFactory;
            this.interviewMultimediaViewStorage = interviewMultimediaViewStorage;
            this.imagesStorage = imagesStorage;
            this.logoSynchronizer = logoSynchronizer;
            this.cleanupService = cleanupService;
            this.assignmentsSynchronizer = assignmentsSynchronizer;
            this.questionnaireDownloader = questionnaireDownloader;
            this.httpStatistician = httpStatistician;
            this.assignmentsStorage = assignmentsStorage;
            this.audioFileStorage = audioFileStorage;
            this.diagnosticService = diagnosticService;
            this.auditLogSynchronizer = auditLogSynchronizer;
            this.eventBus = eventBus;
            this.eventStore = eventStore;
        }

        
        protected override bool SendStatistics => true;
        protected override string SucsessDescription => InterviewerUIResources.Synchronization_Success_Description;


        protected async Task UpdateApplicationAsync(IProgress<SyncProgressInfo> progress,
            CancellationToken cancellationToken)
        {
            if (!await this.synchronizationService.IsAutoUpdateEnabledAsync(cancellationToken))
                return;

            progress.Report(new SyncProgressInfo
            {
                Title = InterviewerUIResources.Synchronization_CheckNewVersionOfApplication,
                Status = SynchronizationStatus.Started
            });

            var versionFromServer = await
                this.synchronizationService.GetLatestApplicationVersionAsync(cancellationToken);

            if (versionFromServer.HasValue && versionFromServer > GetApplicationVersionCode())
            {
                Stopwatch sw = null;
                try
                {
                    await this.diagnosticService.UpdateTheApp(cancellationToken, false, new Progress<TransferProgress>(downloadProgress =>
                    {
                        if (sw == null) sw = Stopwatch.StartNew();
                        if (downloadProgress.ProgressPercentage % 1 != 0) return;

                        var receivedKilobytes = downloadProgress.BytesReceived.Bytes();
                        var totalKilobytes = (downloadProgress.TotalBytesToReceive ?? 0).Bytes();

                        progress.Report(new SyncProgressInfo
                        {
                            Title = InterviewerUIResources.Synchronization_DownloadApplication,
                            Description = string.Format(
                                InterviewerUIResources.Synchronization_DownloadApplication_Description,
                                receivedKilobytes.Humanize("00.00 MB"),
                                totalKilobytes.Humanize("00.00 MB"),
                                receivedKilobytes.Per(sw.Elapsed).Humanize("00.00"),
                                (int) downloadProgress.ProgressPercentage),
                            Status = SynchronizationStatus.Download
                        });
                    }));
                }
                catch (Exception exc)
                {
                    this.logger.Error("Error on auto updating", exc);
                }
            }
        }

        protected async Task CheckObsoleteQuestionnairesAsync(IProgress<SyncProgressInfo> progress,
            SynchronizationStatistics statistics, CancellationToken cancellationToken)
        {
            progress.Report(new SyncProgressInfo
            {
                Title = InterviewerUIResources.Synchronization_Check_Obsolete_Questionnaires,
                Statistics = statistics,
                Status = SynchronizationStatus.Download
            });

            var serverQuestionnaires = await this.synchronizationService.GetServerQuestionnairesAsync(cancellationToken);
            var localQuestionnaires = this.questionnairesAccessor.GetAllQuestionnaireIdentities();

            var questionnairesToRemove = localQuestionnaires.Except(serverQuestionnaires).ToList();

            var removedQuestionnairesCounter = 0;
            foreach (var questionnaireIdentity in questionnairesToRemove)
            {
                cancellationToken.ThrowIfCancellationRequested();
                removedQuestionnairesCounter++;

                progress.Report(new SyncProgressInfo
                {
                    Title = InterviewerUIResources.Synchronization_Check_Obsolete_Questionnaires,
                    Description = string.Format(
                        InterviewerUIResources.Synchronization_Check_Obsolete_Questionnaires_Description,
                        removedQuestionnairesCounter, questionnairesToRemove.Count),
                    Statistics = statistics,
                    Status = SynchronizationStatus.Download
                });

                var questionnaireId = questionnaireIdentity.ToString();

                var removedInterviews = this.interviewViewRepository
                    .Where(interview => interview.QuestionnaireId == questionnaireId)
                    .Select(interview => interview.InterviewId)
                    .ToList();
                this.RemoveInterviews(removedInterviews, statistics, progress);

                this.questionnairesAccessor.RemoveQuestionnaire(questionnaireIdentity);
            }

            if (questionnairesToRemove.Count > 0)
            {
                progress.Report(new SyncProgressInfo
                {
                    Title = InterviewerUIResources.Synchronization_Download_AttachmentsCleanup
                });

                this.cleanupService.RemovedOrphanedAttachments();
            }
        }

        private async Task CreateInterviewsAsync(List<InterviewApiView> interviews,
            SynchronizationStatistics statistics,
            IProgress<SyncProgressInfo> progress,
            CancellationToken cancellationToken)
        {
            statistics.TotalNewInterviewsCount = interviews.Count(interview => !interview.IsRejected);
            statistics.TotalRejectedInterviewsCount = interviews.Count(interview => interview.IsRejected);

            foreach (var interview in interviews)
                try
                {
                    cancellationToken.ThrowIfCancellationRequested();
                    progress.Report(new SyncProgressInfo
                    {
                        Title = InterviewerUIResources.Synchronization_Download_Title,
                        Description = string.Format(InterviewerUIResources.Synchronization_Download_Description_Format,
                            statistics.RejectedInterviewsCount + statistics.NewInterviewsCount + 1, interviews.Count,
                            InterviewerUIResources.Synchronization_Interviews)
                    });

                    await this.questionnaireDownloader.DownloadQuestionnaireAsync(interview.QuestionnaireIdentity, cancellationToken, statistics);

                    List<CommittedEvent> interviewDetails = await this.synchronizationService.GetInterviewDetailsAsync(
                        interview.Id,
                        new Progress<TransferProgress>(),  //TODO: CHANGE
                        //(progressPercentage, bytesReceived, totalBytesToReceive) => { },
                        cancellationToken);

                    if (interviewDetails == null)
                    {
                        statistics.NewInterviewsCount++;
                        continue;
                    }

                    eventBus.PublishCommittedEvents(interviewDetails);
                    eventStore.StoreEvents(new CommittedEventStream(interview.Id, interviewDetails));

                    await this.synchronizationService.LogInterviewAsSuccessfullyHandledAsync(interview.Id);

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

                    await this.TrySendUnexpectedExceptionToServerAsync(exception, cancellationToken);
                    this.logger.Error(
                        $"Failed to create interview {interview.Id}, interviewer {this.principal.CurrentUserIdentity.Name}",
                        exception);
                }
        }

        public async Task DownloadInterviewsAsync(SynchronizationStatistics statistics,
            IProgress<SyncProgressInfo> progress, CancellationToken cancellationToken)
        {
            var remoteInterviews = await this.synchronizationService.GetInterviewsAsync(cancellationToken);

            var remoteInterviewIds = remoteInterviews.Select(interview => interview.Id);

            var localInterviews = this.interviewViewRepository.LoadAll();

            var localInterviewIds = localInterviews.Select(interview => interview.InterviewId).ToHashSet();

            var localInterviewsToRemove = localInterviews.Where(
                interview => !remoteInterviewIds.Contains(interview.InterviewId) && !interview.CanBeDeleted);

            IEnumerable<Guid> obsoleteInterviews = await this.FindObsoleteInterviewsAsync(localInterviewIds, progress, cancellationToken);
            obsoleteInterviews = obsoleteInterviews.Concat(
                remoteInterviews.Where(x => localInterviews.Any(local => local.InterviewId == x.Id && local.ResponsibleId != x.ResponsibleId))
                    .Select(x => x.Id)
            );

            var localInterviewIdsToRemove = localInterviewsToRemove.Select(interview => interview.InterviewId)
                                                                   .Concat(obsoleteInterviews)
                                                                   .ToList();

            var remoteInterviewsToCreate = remoteInterviews
                .Where(interview => !localInterviewIds.Contains(interview.Id) || obsoleteInterviews.Contains(interview.Id))
                .ToList();

            this.RemoveInterviews(localInterviewIdsToRemove, statistics, progress);

            await this.CreateInterviewsAsync(remoteInterviewsToCreate, statistics, progress, cancellationToken);

        }

        protected virtual Task<List<Guid>> FindObsoleteInterviewsAsync(IEnumerable<Guid> localInterviewIds, 
            IProgress<SyncProgressInfo> progress, 
            CancellationToken cancellationToken)
        {
            progress.Report(new SyncProgressInfo
            {
                Title = InterviewerUIResources.Synchronization_CheckForObsolete_Interviews
            });

            var lastKnownEventsWithInterviewIds = localInterviewIds.Select(x => new ObsoletePackageCheck
                {
                    InterviewId = x,
                    SequenceOfLastReceivedEvent = this.eventStore.GetLastEventKnownToHq(x)
                }).Where(x => x.SequenceOfLastReceivedEvent > 0)
                .ToList();
            return this.synchronizationService.CheckObsoleteInterviewsAsync(lastKnownEventsWithInterviewIds, cancellationToken);
        }

        private void RemoveInterviews(List<Guid> interviewIds, SynchronizationStatistics statistics,
            IProgress<SyncProgressInfo> progress)
        {
            statistics.TotalDeletedInterviewsCount += interviewIds.Count;
            foreach (var interviewId in interviewIds)
            {
                progress.Report(new SyncProgressInfo
                {
                    Title = InterviewerUIResources.Synchronization_Download_Title,
                    Description = string.Format(InterviewerUIResources.Synchronization_Download_Description_Format,
                        statistics.DeletedInterviewsCount + 1,
                        interviewIds.Count,
                        InterviewerUIResources.Synchronization_Interviews)
                });

                this.interviewFactory.RemoveInterview(interviewId);
                statistics.DeletedInterviewsCount++;
            }
        }

        protected virtual async Task SyncronizeCensusQuestionnaires(IProgress<SyncProgressInfo> progress,
            SynchronizationStatistics statistics,
            CancellationToken cancellationToken)
        {
            var remoteCensusQuestionnaireIdentities = await this.synchronizationService.GetCensusQuestionnairesAsync(cancellationToken);
            var localCensusQuestionnaireIdentities = this.questionnairesAccessor.GetCensusQuestionnaireIdentities();

            var processedQuestionnaires = 0;
            var notExistingLocalCensusQuestionnaireIdentities = remoteCensusQuestionnaireIdentities
                .Except(localCensusQuestionnaireIdentities).ToList();

            foreach (var censusQuestionnaireIdentity in notExistingLocalCensusQuestionnaireIdentities)
            {
                cancellationToken.ThrowIfCancellationRequested();
                progress.Report(new SyncProgressInfo
                {
                    Title = InterviewerUIResources.Synchronization_Download_Title,
                    Description = string.Format(InterviewerUIResources.Synchronization_Download_Description_Format,
                        processedQuestionnaires,
                        notExistingLocalCensusQuestionnaireIdentities.Count,
                        InterviewerUIResources.Synchronization_Questionnaires)
                });

                await this.questionnaireDownloader.DownloadQuestionnaireAsync(censusQuestionnaireIdentity, cancellationToken, statistics);

                processedQuestionnaires++;
            }
        }
        

        public override SyncStatisticsApiView ToSyncStatisticsApiView(SynchronizationStatistics statistics, Stopwatch stopwatch)
        {
            var httpStats = this.httpStatistician.GetStats();

            return new SyncStatisticsApiView
            {
                DownloadedInterviewsCount = statistics.NewInterviewsCount,
                UploadedInterviewsCount = statistics.SuccessfullyUploadedInterviewsCount,
                DownloadedQuestionnairesCount = statistics.SuccessfullyDownloadedQuestionnairesCount,
                RejectedInterviewsOnDeviceCount =
                    this.interviewViewRepository.Count(
                        inteview => inteview.Status == InterviewStatus.RejectedBySupervisor),
                NewInterviewsOnDeviceCount =
                    this.interviewViewRepository.Count(
                        inteview => inteview.Status == InterviewStatus.InterviewerAssigned && !inteview.CanBeDeleted),
                RemovedInterviewsCount = statistics.DeletedInterviewsCount,

                NewAssignmentsCount = statistics.NewAssignmentsCount,
                RemovedAssignmentsCount = statistics.RemovedAssignmentsCount,
                AssignmentsOnDeviceCount = this.assignmentsStorage.Count(),

                TotalDownloadedBytes = httpStats.DownloadedBytes,
                TotalUploadedBytes = httpStats.UploadedBytes,
                TotalConnectionSpeed = httpStats.Speed,
                TotalSyncDuration = stopwatch.Elapsed
            };
        }

        protected async Task UploadInterviewsAsync(IProgress<SyncProgressInfo> progress,
            SynchronizationStatistics statistics, CancellationToken cancellationToken)
        {
            var completedInterviews = GetInterviewsForUpload();

            statistics.TotalCompletedInterviewsCount = completedInterviews.Count;

            foreach (var completedInterview in completedInterviews)
            {
                cancellationToken.ThrowIfCancellationRequested();
                try
                {
                    var interviewPackage = this.interviewFactory.GetInteviewEventsPackageOrNull(completedInterview.InterviewId);

                    progress.Report(new SyncProgressInfo
                    {
                        Title = string.Format(InterviewerUIResources.Synchronization_Upload_Title_Format,
                            InterviewerUIResources.Synchronization_Upload_CompletedAssignments_Text),
                        Description = string.Format(InterviewerUIResources.Synchronization_Upload_Description_Format,
                            statistics.SuccessfullyUploadedInterviewsCount, statistics.TotalCompletedInterviewsCount,
                            InterviewerUIResources.Synchronization_Upload_Interviews_Text),
                        Status = SynchronizationStatus.Upload
                    });

                    await this.UploadImagesByCompletedInterviewAsync(completedInterview.InterviewId, progress, cancellationToken);
                    await this.UploadAudioByCompletedInterviewAsync(completedInterview.InterviewId, progress, cancellationToken);

                    if (interviewPackage != null)
                    {
                        await this.synchronizationService.UploadInterviewAsync(
                            completedInterview.InterviewId,
                            interviewPackage,
                            new Progress<TransferProgress>(),
                            cancellationToken);
                    }
                    else
                    {
                        this.logger.Warn($"Interview event stream is missing. No package was sent to server. Interview ID: {completedInterview.InterviewId}");
                    }

                    this.interviewFactory.RemoveInterview(completedInterview.InterviewId);
                    statistics.SuccessfullyUploadedInterviewsCount++;
                }
                catch (OperationCanceledException)
                {

                }
                catch (Exception syncException)
                {
                    statistics.FailedToUploadInterviwesCount++;
                    await this.TrySendUnexpectedExceptionToServerAsync(syncException, cancellationToken);

                    this.logger.Error($"Failed to sync interview {completedInterview.Id}. Interviewer login {this.principal.CurrentUserIdentity.Name}", syncException);
                }
            }
        }

        protected abstract IReadOnlyCollection<InterviewView> GetInterviewsForUpload();

        private async Task UploadImagesByCompletedInterviewAsync(Guid interviewId, IProgress<SyncProgressInfo> progress,
            CancellationToken cancellationToken)
        {
            var imageViews = this.interviewMultimediaViewStorage.Where(image => image.InterviewId == interviewId);

            foreach (var imageView in imageViews)
            {
                cancellationToken.ThrowIfCancellationRequested();
                var fileView = this.imagesStorage.GetById(imageView.FileId);
                await this.synchronizationService.UploadInterviewImageAsync(
                    imageView.InterviewId,
                    imageView.FileName,
                    fileView.File,
                    new Progress<TransferProgress>(),
                    cancellationToken);
                this.interviewMultimediaViewStorage.Remove(imageView.Id);
                this.imagesStorage.Remove(fileView.Id);
            }
        }

        private async Task UploadAudioByCompletedInterviewAsync(Guid interviewId, IProgress<SyncProgressInfo> progress,
            CancellationToken cancellationToken)
        {
            var audioFiles = this.audioFileStorage.GetBinaryFilesForInterview(interviewId);

            foreach (var audioFile in audioFiles)
            {
                cancellationToken.ThrowIfCancellationRequested();
                var fileData = audioFile.GetData();
                await this.synchronizationService.UploadInterviewAudioAsync(
                    audioFile.InterviewId,
                    audioFile.FileName,
                    audioFile.ContentType,
                    fileData,
                    new Progress<TransferProgress>(),
                    cancellationToken);
                this.audioFileStorage.RemoveInterviewBinaryData(audioFile.InterviewId, audioFile.FileName);
            }
        }

        protected abstract int GetApplicationVersionCode();
    }
}
