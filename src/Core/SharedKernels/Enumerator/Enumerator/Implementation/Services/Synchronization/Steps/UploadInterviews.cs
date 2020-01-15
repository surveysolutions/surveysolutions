using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using WB.Core.GenericSubdomains.Portable.Services;
using WB.Core.SharedKernels.DataCollection.Repositories;
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

        protected UploadInterviews(IInterviewerInterviewAccessor interviewFactory,
            IPlainStorage<InterviewMultimediaView> interviewMultimediaViewStorage,
            ILogger logger,
            IImageFileStorage imagesStorage,
            IAudioFileStorage audioFileStorage,
            ISynchronizationService synchronizationService,
            IAudioAuditFileStorage audioAuditFileStorage,
            int sortOrder) : base(sortOrder, synchronizationService, logger)
        {
            this.interviewFactory = interviewFactory ?? throw new ArgumentNullException(nameof(interviewFactory));
            this.interviewMultimediaViewStorage = interviewMultimediaViewStorage ?? throw new ArgumentNullException(nameof(interviewMultimediaViewStorage));
            this.imagesStorage = imagesStorage ?? throw new ArgumentNullException(nameof(imagesStorage));
            this.audioFileStorage = audioFileStorage ?? throw new ArgumentNullException(nameof(audioFileStorage));
            this.audioAuditFileStorage = audioAuditFileStorage;
        }

        public override async Task ExecuteAsync()
        {
            var interviewsToUpload = GetInterviewsForUpload();

            Context.Statistics.TotalCompletedInterviewsCount = interviewsToUpload.Count;
            var transferProgress = Context.Progress.AsTransferReport();

            foreach (var completedInterview in interviewsToUpload)
            {
                Context.CancellationToken.ThrowIfCancellationRequested();
                try
                {
                    Context.Progress.Report(new SyncProgressInfo
                    {
                        Title = string.Format(EnumeratorUIResources.Synchronization_Upload_Title_Format,
                            EnumeratorUIResources.Synchronization_Upload_CompletedAssignments_Text),
                        Description = string.Format(EnumeratorUIResources.Synchronization_Upload_Description_Format,
                            Context.Statistics.SuccessfullyUploadedInterviewsCount, Context.Statistics.TotalCompletedInterviewsCount,
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

                    var interviewEventStreamContainer = this.interviewFactory.GetInterviewEventStreamContainer(completedInterview.InterviewId);

                    var uploadState = await this.synchronizationService.GetInterviewUploadState(completedInterview.InterviewId,
                        interviewEventStreamContainer.Tag, Context.CancellationToken);

                    await this.UploadImagesByCompletedInterviewAsync(completedInterview.InterviewId, uploadState,
                        Context.Progress, Context.CancellationToken);

                    await this.UploadAudioByCompletedInterviewAsync(completedInterview.InterviewId, uploadState,
                        Context.Progress, Context.CancellationToken);

                    await this.UploadAudioAuditByCompletedInterviewAsync(completedInterview.InterviewId, uploadState,
                        Context.Progress, Context.CancellationToken);

                    if (!uploadState.IsEventsUploaded)
                    {
                        var interviewPackage = this.interviewFactory.GetInterviewEventsPackageOrNull(interviewEventStreamContainer);

                        if (interviewPackage != null)
                        {
                            await this.synchronizationService.UploadInterviewAsync(
                                completedInterview.InterviewId,
                                interviewPackage,
                                transferProgress,
                                Context.CancellationToken);
                        }
                        else
                        {
                            this.logger.Error($"Interview event stream is missing. No package was sent to server");
                            throw new Exception($"Data inconsistency for interview {completedInterview.InterviewId}. No events to send.");
                        }
                    }
                    else
                    {
                        this.logger.Warn("Interview event stream is already uploaded");
                    }

                    this.interviewFactory.RemoveInterview(completedInterview.InterviewId);
                    this.Context.Statistics.SuccessfullyUploadedInterviewsCount++;
                }
                catch (Exception syncException)
                {
                    this.Context.Statistics.FailedToUploadInterviewsCount++;
                    await base.TrySendUnexpectedExceptionToServerAsync(syncException);

                    this.logger.Error($"Failed to synchronize interview", syncException);
                }
            }
        }
        private async Task UploadImagesByCompletedInterviewAsync(Guid interviewId, InterviewUploadState uploadState,
            IProgress<SyncProgressInfo> progress,
            CancellationToken cancellationToken)
        {
            var imageViews = this.interviewMultimediaViewStorage.Where(image => image.InterviewId == interviewId);
            var transferProgress = progress.AsTransferReport();

            foreach (var imageView in imageViews)
            {
                if (uploadState.ImagesFilesNames.Contains(imageView.FileName)) continue;

                cancellationToken.ThrowIfCancellationRequested();

                await this.synchronizationService.UploadInterviewImageAsync(
                    imageView.InterviewId,
                    imageView.FileName,
                    await this.imagesStorage.GetInterviewBinaryDataAsync(interviewId, imageView.FileName),
                    transferProgress,
                    cancellationToken);

                this.interviewMultimediaViewStorage.Remove(imageView.Id);
                await this.imagesStorage.RemoveInterviewBinaryData(interviewId, imageView.FileName);
            }
        }
        
        private async Task UploadAudioAuditByCompletedInterviewAsync(Guid interviewId, InterviewUploadState uploadState,
            IProgress<SyncProgressInfo> progress,
            CancellationToken cancellationToken)
        {
            var auditFiles = await this.audioAuditFileStorage.GetBinaryFilesForInterview(interviewId);
            var transferProgress = progress.AsTransferReport();

            foreach (var auditFile in auditFiles)
            {
                if (uploadState.AudioFilesNames.Contains(auditFile.FileName)) continue;

                cancellationToken.ThrowIfCancellationRequested();
                var fileData = await auditFile.GetData();

                await this.synchronizationService.UploadInterviewAudioAuditAsync(
                    auditFile.InterviewId,
                    auditFile.FileName,
                    auditFile.ContentType,
                    fileData,
                    transferProgress,
                    cancellationToken);

                await this.audioAuditFileStorage.RemoveInterviewBinaryData(auditFile.InterviewId, auditFile.FileName);
            }
        }

        private async Task UploadAudioByCompletedInterviewAsync(Guid interviewId, InterviewUploadState uploadState,
            IProgress<SyncProgressInfo> progress,
            CancellationToken cancellationToken)
        {
            var audioFiles = await this.audioFileStorage.GetBinaryFilesForInterview(interviewId);
            var transferProgress = progress.AsTransferReport();

            foreach (var audioFile in audioFiles)
            {
                if (uploadState.AudioFilesNames.Contains(audioFile.FileName)) continue;

                cancellationToken.ThrowIfCancellationRequested();
                var fileData = await audioFile.GetData();

                await this.synchronizationService.UploadInterviewAudioAsync(
                    audioFile.InterviewId,
                    audioFile.FileName,
                    audioFile.ContentType,
                    fileData,
                    transferProgress,
                    cancellationToken);

                await this.audioFileStorage.RemoveInterviewBinaryData(audioFile.InterviewId, audioFile.FileName);
            }
        }

        protected abstract IReadOnlyCollection<InterviewView> GetInterviewsForUpload();
    }
}
