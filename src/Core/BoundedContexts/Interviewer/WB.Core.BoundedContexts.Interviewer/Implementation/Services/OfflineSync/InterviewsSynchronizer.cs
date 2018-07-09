using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using WB.Core.SharedKernels.DataCollection.Repositories;
using WB.Core.SharedKernels.DataCollection.ValueObjects.Interview;
using WB.Core.SharedKernels.Enumerator.OfflineSync.Messages;
using WB.Core.SharedKernels.Enumerator.OfflineSync.Services;
using WB.Core.SharedKernels.Enumerator.Services.Infrastructure;
using WB.Core.SharedKernels.Enumerator.Services.Infrastructure.Storage;
using WB.Core.SharedKernels.Enumerator.Views;

namespace WB.Core.BoundedContexts.Interviewer.Implementation.Services.OfflineSync
{
    public class InterviewsSynchronizer : IInterviewsSynchronizer
    {
        private readonly IPlainStorage<InterviewView> interviewStorage;
        private readonly IInterviewerInterviewAccessor interviewAccessor;
        private readonly IOfflineSyncClient syncClient;
        private readonly IAudioFileStorage audioFileStorage;
        private readonly IPlainStorage<InterviewMultimediaView> interviewMultimediaViewStorage;
        private readonly IPlainStorage<InterviewFileView> imagesStorage;

        public InterviewsSynchronizer(IPlainStorage<InterviewView> interviewStorage,
            IInterviewerInterviewAccessor interviewAccessor,
            IOfflineSyncClient syncClient,
            IAudioFileStorage audioFileStorage,
            IPlainStorage<InterviewMultimediaView> interviewMultimediaViewStorage,
            IPlainStorage<InterviewFileView> imagesStorage)
        {
            this.interviewStorage = interviewStorage;
            this.interviewAccessor = interviewAccessor;
            this.syncClient = syncClient;
            this.audioFileStorage = audioFileStorage;
            this.interviewMultimediaViewStorage = interviewMultimediaViewStorage;
            this.imagesStorage = imagesStorage;
        }

        public async Task UploadPendingInterviews(IProgress<UploadProgress> progress, CancellationToken cancellationToken)
        {
            var interviews = this.interviewStorage.Where(interview => interview.Status == InterviewStatus.Completed).ToArray();
            for (int i = 0; i < interviews.Length; i++)
            {
                var postedInterviewId = interviews[i].InterviewId;

                await this.UploadImagesByCompletedInterviewAsync(postedInterviewId, progress, cancellationToken);
                await this.UploadAudioByCompletedInterviewAsync(postedInterviewId, progress, cancellationToken);

                var interviewPackage = this.interviewAccessor.GetPendingInteviewEvents(postedInterviewId);
                await this.syncClient.SendAsync(new PostInterviewRequest(postedInterviewId, interviewPackage), cancellationToken);
                this.interviewAccessor.RemoveInterview(postedInterviewId);

                progress.Report(new UploadProgress(i + 1, interviews.Length));
            }
        }

        private async Task UploadImagesByCompletedInterviewAsync(Guid interviewId, IProgress<UploadProgress> progress,
            CancellationToken cancellationToken)
        {
            var imageViews = this.interviewMultimediaViewStorage.Where(image => image.InterviewId == interviewId);

            foreach (var imageView in imageViews)
            {
                cancellationToken.ThrowIfCancellationRequested();
                var fileView = this.imagesStorage.GetById(imageView.FileId);

                await this.syncClient.SendAsync(new PostInterviewImageRequest(
                    imageView.InterviewId,
                    imageView.FileName,
                    fileView.File), cancellationToken);

                this.interviewMultimediaViewStorage.Remove(imageView.Id);
                this.imagesStorage.Remove(fileView.Id);
            }
        }

        private async Task UploadAudioByCompletedInterviewAsync(Guid interviewId, IProgress<UploadProgress> progress,
            CancellationToken cancellationToken)
        {
            var audioFiles = this.audioFileStorage.GetBinaryFilesForInterview(interviewId);

            foreach (var audioFile in audioFiles)
            {
                cancellationToken.ThrowIfCancellationRequested();
                var fileData = audioFile.GetData();

                await this.syncClient.SendAsync(new PostInterviewAudioRequest(
                    interviewId,
                    audioFile.FileName,
                    audioFile.ContentType,
                    fileData), cancellationToken);

                this.audioFileStorage.RemoveInterviewBinaryData(audioFile.InterviewId, audioFile.FileName);
            }
        }
    }
}
