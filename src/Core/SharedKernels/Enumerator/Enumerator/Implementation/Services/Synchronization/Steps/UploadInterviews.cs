using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using WB.Core.GenericSubdomains.Portable.Services;
using WB.Core.SharedKernels.DataCollection.Repositories;
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
        private readonly IPlainStorage<InterviewFileView> imagesStorage;
        private readonly IAudioFileStorage audioFileStorage;
        private readonly ISynchronizationService synchronizationService;
        private readonly ILogger logger;

        public UploadInterviews(IInterviewerInterviewAccessor interviewFactory, 
            IPlainStorage<InterviewMultimediaView> interviewMultimediaViewStorage, 
            ILogger logger, 
            IPlainStorage<InterviewFileView> imagesStorage,
            IAudioFileStorage audioFileStorage,
            ISynchronizationService synchronizationService,
            int sortOrder) : base(sortOrder, synchronizationService, logger)
        {
            this.interviewFactory = interviewFactory ?? throw new ArgumentNullException(nameof(interviewFactory));
            this.interviewMultimediaViewStorage = interviewMultimediaViewStorage ?? throw new ArgumentNullException(nameof(interviewMultimediaViewStorage));
            this.logger = logger ?? throw new ArgumentNullException(nameof(logger));
            this.imagesStorage = imagesStorage ?? throw new ArgumentNullException(nameof(imagesStorage));
            this.audioFileStorage = audioFileStorage ?? throw new ArgumentNullException(nameof(audioFileStorage));
            this.synchronizationService = synchronizationService ?? throw new ArgumentNullException(nameof(synchronizationService));
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
                    var interviewPackage = this.interviewFactory.GetInteviewEventsPackageOrNull(completedInterview.InterviewId);

                    Context.Progress.Report(new SyncProgressInfo
                    {
                        Title = string.Format(InterviewerUIResources.Synchronization_Upload_Title_Format,
                            InterviewerUIResources.Synchronization_Upload_CompletedAssignments_Text),
                        Description = string.Format(InterviewerUIResources.Synchronization_Upload_Description_Format,
                            Context.Statistics.SuccessfullyUploadedInterviewsCount, Context.Statistics.TotalCompletedInterviewsCount,
                            InterviewerUIResources.Synchronization_Upload_Interviews_Text),
                        Status = SynchronizationStatus.Upload,
                        Stage = SyncStage.UploadInterviews,
                        Statistics = Context.Statistics,

                        StageExtraInfo = new Dictionary<string, string>()
                        {
                            { "processedCount", Context.Statistics.SuccessfullyUploadedInterviewsCount.ToString() },
                            { "totalCount", interviewsToUpload.Count.ToString()}
                        }
                    });

                    await this.UploadImagesByCompletedInterviewAsync(completedInterview.InterviewId, Context.Progress, Context.CancellationToken);
                    await this.UploadAudioByCompletedInterviewAsync(completedInterview.InterviewId, Context.Progress, Context.CancellationToken);

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
                        this.logger.Warn($"Interview event stream is missing. No package was sent to server");
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

        private async Task UploadImagesByCompletedInterviewAsync(Guid interviewId, IProgress<SyncProgressInfo> progress,
            CancellationToken cancellationToken)
        {
            var imageViews = this.interviewMultimediaViewStorage.Where(image => image.InterviewId == interviewId);
            var transferProgress = progress.AsTransferReport();

            foreach (var imageView in imageViews)
            {
                cancellationToken.ThrowIfCancellationRequested();
                var fileView = this.imagesStorage.GetById(imageView.FileId);
                await this.synchronizationService.UploadInterviewImageAsync(
                    imageView.InterviewId,
                    imageView.FileName,
                    fileView.File,
                    transferProgress,
                    cancellationToken);
                this.interviewMultimediaViewStorage.Remove(imageView.Id);
                this.imagesStorage.Remove(fileView.Id);
            }
        }

        private async Task UploadAudioByCompletedInterviewAsync(Guid interviewId, IProgress<SyncProgressInfo> progress,
            CancellationToken cancellationToken)
        {
            var audioFiles = this.audioFileStorage.GetBinaryFilesForInterview(interviewId);
            var transferProgress = progress.AsTransferReport();

            foreach (var audioFile in audioFiles)
            {
                cancellationToken.ThrowIfCancellationRequested();
                var fileData = audioFile.GetData();
                await this.synchronizationService.UploadInterviewAudioAsync(
                    audioFile.InterviewId,
                    audioFile.FileName,
                    audioFile.ContentType,
                    fileData,
                    transferProgress,
                    cancellationToken);
                this.audioFileStorage.RemoveInterviewBinaryData(audioFile.InterviewId, audioFile.FileName);
            }
        }

        protected abstract IReadOnlyCollection<InterviewView> GetInterviewsForUpload();
    }
}
