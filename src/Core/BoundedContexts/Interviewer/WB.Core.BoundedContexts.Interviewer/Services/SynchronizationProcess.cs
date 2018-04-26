using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Humanizer;
using Humanizer.Bytes;
using WB.Core.BoundedContexts.Interviewer.Properties;
using WB.Core.BoundedContexts.Interviewer.Services.Infrastructure;
using WB.Core.BoundedContexts.Interviewer.Services.Synchronization;
using WB.Core.BoundedContexts.Interviewer.Views;
using WB.Core.BoundedContexts.Interviewer.Views.Dashboard;
using WB.Core.GenericSubdomains.Portable;
using WB.Core.GenericSubdomains.Portable.Services;
using WB.Core.SharedKernels.DataCollection.Repositories;
using WB.Core.SharedKernels.DataCollection.ValueObjects.Interview;
using WB.Core.SharedKernels.DataCollection.WebApi;
using WB.Core.SharedKernels.Enumerator.Services;
using WB.Core.SharedKernels.Enumerator.Services.Infrastructure.Storage;

namespace WB.Core.BoundedContexts.Interviewer.Services
{
    public class SynchronizationProcess : AbstractSynchronizationProcess, ISynchronizationProcess
    {
        private readonly AttachmentsCleanupService cleanupService;
        private readonly IHttpStatistician httpStatistician;
        private readonly IPlainStorage<AssignmentDocument, int> assignmentsStorage;
        private readonly IInterviewerInterviewAccessor interviewFactory;
        private readonly IAudioFileStorage audioFileStorage;
        private readonly ITabletDiagnosticService diagnosticService;
        private readonly IInterviewerSettings interviewerSettings;
        private readonly IAuditLogSynchronizer auditLogSynchronizer;
        private readonly IPlainStorage<InterviewFileView> imagesStorage;
        private readonly IPlainStorage<InterviewMultimediaView> interviewMultimediaViewStorage;
        private readonly IPlainStorage<InterviewView> interviewViewRepository;
        private readonly ILogger logger;
        private readonly CompanyLogoSynchronizer logoSynchronizer;
        private readonly IAssignmentsSynchronizer assignmentsSynchronizer;
        private readonly IQuestionnaireDownloader questionnaireDownloader;
        private readonly IInterviewerPrincipal principal;
        private readonly IInterviewerQuestionnaireAccessor questionnairesAccessor;
        private readonly ISynchronizationService synchronizationService;

        public SynchronizationProcess(ISynchronizationService synchronizationService,
            IPlainStorage<InterviewerIdentity> interviewersPlainStorage,
            IPlainStorage<InterviewView> interviewViewRepository,
            IInterviewerPrincipal principal,
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
            IPlainStorage<AssignmentDocument, int> assignmentsStorage,
            IAudioFileStorage audioFileStorage,
            ITabletDiagnosticService diagnosticService,
            IInterviewerSettings interviewerSettings,
            IAuditLogSynchronizer auditLogSynchronizer) : base(synchronizationService, logger,
            httpStatistician, userInteractionService, principal, passwordHasher, interviewersPlainStorage,
            interviewViewRepository)
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
            this.interviewerSettings = interviewerSettings;
            this.auditLogSynchronizer = auditLogSynchronizer;
        }

        
        protected override bool SendStatistics => true;
        protected override string SucsessDescription => InterviewerUIResources.Synchronization_Success_Description;

        public override async Task Synchronize(IProgress<SyncProgressInfo> progress, CancellationToken cancellationToken, SynchronizationStatistics statistics)
        {
            cancellationToken.ThrowIfCancellationRequested();
            await this.UploadCompletedInterviewsAsync(progress, statistics, cancellationToken);

            cancellationToken.ThrowIfCancellationRequested();
            await this.assignmentsSynchronizer.SynchronizeAssignmentsAsync(progress, statistics, cancellationToken);

            cancellationToken.ThrowIfCancellationRequested();
            await this.SyncronizeCensusQuestionnaires(progress, statistics, cancellationToken);

            cancellationToken.ThrowIfCancellationRequested();
            await this.CheckObsoleteQuestionnairesAsync(progress, statistics, cancellationToken);

            cancellationToken.ThrowIfCancellationRequested();
            await this.DownloadInterviewsAsync(statistics, progress, cancellationToken);

            cancellationToken.ThrowIfCancellationRequested();
            await this.logoSynchronizer.DownloadCompanyLogo(progress, cancellationToken);

            cancellationToken.ThrowIfCancellationRequested();
            await this.auditLogSynchronizer.SynchronizeAuditLogAsync(progress, statistics, cancellationToken);

            cancellationToken.ThrowIfCancellationRequested();
            await this.UpdateApplicationAsync(progress, cancellationToken);
        }

        private async Task UpdateApplicationAsync(IProgress<SyncProgressInfo> progress,
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

            if (versionFromServer.HasValue && versionFromServer > this.interviewerSettings.GetApplicationVersionCode())
            {
                Stopwatch sw = null;
                try
                {
                    await this.diagnosticService.UpdateTheApp(cancellationToken, false, downloadProgress =>
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
                    });
                }
                catch (Exception exc)
                {
                    this.logger.Error("Error on auto updating", exc);
                }
            }
        }

        private async Task CheckObsoleteQuestionnairesAsync(IProgress<SyncProgressInfo> progress,
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

                    var interviewDetails = await this.synchronizationService.GetInterviewDetailsAsync(
                        interview.Id,
                        (progressPercentage, bytesReceived, totalBytesToReceive) => { },
                        cancellationToken);

                    if (interviewDetails == null)
                    {
                        statistics.NewInterviewsCount++;
                        continue;
                    }

                    await this.interviewFactory.CreateInterviewAsync(interview, interviewDetails);
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

        private async Task DownloadInterviewsAsync(SynchronizationStatistics statistics,
            IProgress<SyncProgressInfo> progress, CancellationToken cancellationToken)
        {
            var remoteInterviews = await this.synchronizationService.GetInterviewsAsync(cancellationToken);

            var remoteInterviewIds = remoteInterviews.Select(interview => interview.Id);

            var localInterviews = this.interviewViewRepository.LoadAll();

            var localInterviewIds = localInterviews.Select(interview => interview.InterviewId).ToList();

            var localInterviewsToRemove = localInterviews.Where(
                interview => !remoteInterviewIds.Contains(interview.InterviewId) && !interview.CanBeDeleted);

            var localInterviewIdsToRemove = localInterviewsToRemove.Select(interview => interview.InterviewId).ToList();

            var remoteInterviewsToCreate = remoteInterviews
                .Where(interview => !localInterviewIds.Contains(interview.Id)).ToList();

            this.RemoveInterviews(localInterviewIdsToRemove, statistics, progress);

            await this.CreateInterviewsAsync(remoteInterviewsToCreate, statistics, progress, cancellationToken);
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

        private async Task SyncronizeCensusQuestionnaires(IProgress<SyncProgressInfo> progress,
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

        private async Task UploadCompletedInterviewsAsync(IProgress<SyncProgressInfo> progress,
            SynchronizationStatistics statistics, CancellationToken cancellationToken)
        {
            var completedInterviews =
                this.interviewViewRepository.Where(interview => interview.Status == InterviewStatus.Completed);

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
                            (progressPercentage, bytesReceived, totalBytesToReceive) => { },
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
                    (progressPercentage, bytesReceived, totalBytesToReceive) => { },
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
                    (progressPercentage, bytesReceived, totalBytesToReceive) => { },
                    cancellationToken);
                this.audioFileStorage.RemoveInterviewBinaryData(audioFile.InterviewId, audioFile.FileName);
            }
        }
        
    }
}
