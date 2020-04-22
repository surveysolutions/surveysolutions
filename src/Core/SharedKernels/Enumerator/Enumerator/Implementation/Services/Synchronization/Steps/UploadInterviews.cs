using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Cryptography;
using System.Threading;
using System.Threading.Tasks;
using WB.Core.GenericSubdomains.Portable;
using WB.Core.GenericSubdomains.Portable.Services;
using WB.Core.SharedKernels.DataCollection.Repositories;
using WB.Core.SharedKernels.DataCollection.ValueObjects.Interview;
using WB.Core.SharedKernels.DataCollection.WebApi;
using WB.Core.SharedKernels.Enumerator.Properties;
using WB.Core.SharedKernels.Enumerator.Services.Infrastructure;
using WB.Core.SharedKernels.Enumerator.Services.Infrastructure.Storage;
using WB.Core.SharedKernels.Enumerator.Services.Synchronization;
using WB.Core.SharedKernels.Enumerator.Views;

namespace WB.Core.SharedKernels.Enumerator.Implementation.Services.Synchronization.Steps
{
    public abstract class UploadInterviews : SynchronizationStep
    {
        private readonly IInterviewerInterviewAccessor interviewFactory;
        private readonly IPlainStorage<InterviewMultimediaView> interviewMultimediaViewStorage;
        private readonly IImageFileStorage imagesStorage;
        private readonly IAudioFileStorage audioFileStorage;
        private readonly IAudioAuditFileStorage audioAuditFileStorage;
        private readonly IPlainStorage<InterviewView> interviewViewRepository;

        protected UploadInterviews(IInterviewerInterviewAccessor interviewFactory,
            IPlainStorage<InterviewMultimediaView> interviewMultimediaViewStorage,
            ILogger logger,
            IImageFileStorage imagesStorage,
            IAudioFileStorage audioFileStorage,
            ISynchronizationService synchronizationService,
            IAudioAuditFileStorage audioAuditFileStorage,
            IPlainStorage<InterviewView> interviewViewRepository,
            int sortOrder) : base(sortOrder, synchronizationService, logger)
        {
            this.interviewFactory = interviewFactory ?? throw new ArgumentNullException(nameof(interviewFactory));
            this.interviewMultimediaViewStorage = interviewMultimediaViewStorage ?? throw new ArgumentNullException(nameof(interviewMultimediaViewStorage));
            this.imagesStorage = imagesStorage ?? throw new ArgumentNullException(nameof(imagesStorage));
            this.audioFileStorage = audioFileStorage ?? throw new ArgumentNullException(nameof(audioFileStorage));
            this.audioAuditFileStorage = audioAuditFileStorage;
            this.interviewViewRepository = interviewViewRepository;
        }

        public override async Task ExecuteAsync()
        {
            var interviewsToUpload = GetInterviewsForUpload();
            bool isNeedCompress = IsCompressEnabled();

            var countInterviewsForFullUpload = interviewsToUpload.Count(i => IsNonPartialSynchedInterview(i));
            Context.Statistics.TotalCompletedInterviewsCount = countInterviewsForFullUpload;
            Context.Statistics.TotalPartialUploadedInterviewsCount = interviewsToUpload.Count - countInterviewsForFullUpload;

            var transferProgress = Context.Progress.AsTransferReport();

            foreach (var interview in interviewsToUpload)
            {
                Context.CancellationToken.ThrowIfCancellationRequested();
                try
                {
                    Context.Progress.Report(new SyncProgressInfo
                    {
                        Title = string.Format(EnumeratorUIResources.Synchronization_Upload_Title_Format,
                            EnumeratorUIResources.Synchronization_Upload_CompletedAssignments_Text),
                        Description = string.Format(EnumeratorUIResources.Synchronization_Upload_Description_Format,
                            Context.Statistics.SuccessfullyUploadedInterviewsCount, interviewsToUpload.Count,
                            EnumeratorUIResources.Synchronization_Upload_Interviews_Text),
                        Status = SynchronizationStatus.Upload,
                        Stage = SyncStage.UploadInterviews,
                        Statistics = Context.Statistics,

                        StageExtraInfo = new Dictionary<string, string>()
                        {
                            { "processedCount", Context.Statistics.SuccessfullyUploadedInterviewsCount.ToString() },
                            { "totalCount", interviewsToUpload.Count.ToString()}
                        }
                    });

                    var isPartialSynchedInterview = IsPartialSynchedInterview(interview);
                    if (isPartialSynchedInterview && Context.Statistics.FailToPartialProcessInterviewIds.Contains(interview.InterviewId))
                    {
                        // don't partial synch interviews with problems on download changes and don't have last version of stream
                        continue;
                    }

                    var interviewEventStreamContainer = this.interviewFactory.GetInterviewEventStreamContainer(interview.InterviewId, isNeedCompress);

                    var uploadState = await this.synchronizationService.GetInterviewUploadState(interview.InterviewId,
                        interviewEventStreamContainer.Tag, Context.CancellationToken);

                    await this.UploadImagesByCompletedInterviewAsync(interview, uploadState,
                        Context.Progress, Context.CancellationToken);

                    await this.UploadAudioByCompletedInterviewAsync(interview, uploadState,
                        Context.Progress, Context.CancellationToken);

                    await this.UploadAudioAuditByCompletedInterviewAsync(interview, uploadState,
                        Context.Progress, Context.CancellationToken);

                    if (!uploadState.IsEventsUploaded)
                    {
                        var interviewPackage = this.interviewFactory.GetInterviewEventsPackageOrNull(interviewEventStreamContainer);

                        if (interviewPackage != null)
                        {
                            await this.synchronizationService.UploadInterviewAsync(
                                interview.InterviewId,
                                interviewPackage,
                                transferProgress,
                                Context.CancellationToken);
                        }
                        else
                        {
                            this.logger.Error($"Interview event stream is missing. No package was sent to server");
                            throw new Exception($"Data inconsistency for interview {interview.InterviewId}. No events to send.");
                        }
                    }
                    else
                    {
                        this.logger.Warn("Interview event stream is already uploaded");
                    }

                    if (!isPartialSynchedInterview)
                    {
                        this.interviewFactory.RemoveInterview(interview.InterviewId);
                        this.Context.Statistics.SuccessfullyUploadedInterviewsCount++;
                    }
                    else
                    {
                        this.interviewFactory.MarkEventsAsReceivedByHQ(interview.InterviewId);
                        MarkInterviewAsSynchedAndNonDeletedMore(interview.InterviewId);
                        this.Context.Statistics.SuccessfullyPartialUploadedInterviewsCount++;
                    }
                }
                catch (Exception syncException)
                {
                    this.Context.Statistics.FailedToUploadInterviewsCount++;
                    await base.TrySendUnexpectedExceptionToServerAsync(syncException);

                    this.logger.Error($"Failed to synchronize interview", syncException);
                }
            }
        }

        protected abstract bool IsCompressEnabled();

        protected abstract bool IsNonPartialSynchedInterview(InterviewView interview);

        protected bool IsPartialSynchedInterview(InterviewView interview) => !IsNonPartialSynchedInterview(interview);

        private void MarkInterviewAsSynchedAndNonDeletedMore(Guid interviewId)
        {
            var interviewView = interviewViewRepository.GetById(interviewId.FormatGuid());
            interviewView.CanBeDeleted = false;
            interviewView.FromHqSyncDateTime = DateTime.UtcNow;
            interviewViewRepository.Store(interviewView);
        }

        private async Task UploadImagesByCompletedInterviewAsync(InterviewView interview, InterviewUploadState uploadState,
            IProgress<SyncProgressInfo> progress,
            CancellationToken cancellationToken)
        {
            var imageViews = this.interviewMultimediaViewStorage.Where(image => image.InterviewId == interview.InterviewId);
            var transferProgress = progress.AsTransferReport();

            foreach (var imageView in imageViews)
            {
                if (uploadState.ImagesFilesNames.Contains(imageView.FileName)) continue;

                cancellationToken.ThrowIfCancellationRequested();

                var fileContent = await this.imagesStorage.GetInterviewBinaryDataAsync(interview.InterviewId, imageView.FileName);
                var hash = GetMd5Cache(fileContent);
                if (uploadState.ImageQuestionsFilesMd5?.Contains(hash) ?? false) continue;

                cancellationToken.ThrowIfCancellationRequested();

                await this.synchronizationService.UploadInterviewImageAsync(
                    imageView.InterviewId,
                    imageView.FileName,
                    fileContent,
                    transferProgress,
                    cancellationToken);

                if (interview.Status == InterviewStatus.Completed)
                {
                    this.interviewMultimediaViewStorage.Remove(imageView.Id);
                    await this.imagesStorage.RemoveInterviewBinaryData(interview.InterviewId, imageView.FileName);
                }
            }
        }
        
        private async Task UploadAudioAuditByCompletedInterviewAsync(InterviewView interview, InterviewUploadState uploadState,
            IProgress<SyncProgressInfo> progress,
            CancellationToken cancellationToken)
        {
            var auditFiles = await this.audioAuditFileStorage.GetBinaryFilesForInterview(interview.InterviewId);
            var transferProgress = progress.AsTransferReport();

            foreach (var auditFile in auditFiles)
            {
                if (uploadState.AudioFilesNames.Contains(auditFile.FileName)) continue;

                cancellationToken.ThrowIfCancellationRequested();

                var fileData = await auditFile.GetData();
                var hash = GetMd5Cache(fileData);
                if (uploadState.AudioAuditFilesMd5?.Contains(hash) ?? false) continue;

                cancellationToken.ThrowIfCancellationRequested();

                await this.synchronizationService.UploadInterviewAudioAuditAsync(
                    auditFile.InterviewId,
                    auditFile.FileName,
                    auditFile.ContentType,
                    fileData,
                    transferProgress,
                    cancellationToken);

                if (interview.Status == InterviewStatus.Completed)
                    await this.audioAuditFileStorage.RemoveInterviewBinaryData(auditFile.InterviewId, auditFile.FileName);
            }
        }

        private async Task UploadAudioByCompletedInterviewAsync(InterviewView interview, InterviewUploadState uploadState,
            IProgress<SyncProgressInfo> progress,
            CancellationToken cancellationToken)
        {
            var audioFiles = await this.audioFileStorage.GetBinaryFilesForInterview(interview.InterviewId);
            var transferProgress = progress.AsTransferReport();

            foreach (var audioFile in audioFiles)
            {
                if (uploadState.AudioFilesNames.Contains(audioFile.FileName)) continue;

                cancellationToken.ThrowIfCancellationRequested();
                
                var fileData = await audioFile.GetData();
                var hash = GetMd5Cache(fileData);
                if (uploadState.AudioQuestionsFilesMd5?.Contains(hash) ?? false) continue;

                cancellationToken.ThrowIfCancellationRequested();

                await this.synchronizationService.UploadInterviewAudioAsync(
                    audioFile.InterviewId,
                    audioFile.FileName,
                    audioFile.ContentType,
                    fileData,
                    transferProgress,
                    cancellationToken);

                if (interview.Status == InterviewStatus.Completed)
                    await this.audioFileStorage.RemoveInterviewBinaryData(audioFile.InterviewId, audioFile.FileName);
            }
        }

        private static string GetMd5Cache(byte[] content)
        {
            using var crypto = MD5.Create();
            var hash = crypto.ComputeHash(content);
            var hashString = BitConverter.ToString(hash).Replace("-", "").ToLowerInvariant();
            return hashString;
        }


        protected abstract IReadOnlyCollection<InterviewView> GetInterviewsForUpload();
    }
}
