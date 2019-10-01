﻿using System.Linq;
using System.Threading.Tasks;
using WB.Core.BoundedContexts.Supervisor.Views;
using WB.Core.GenericSubdomains.Portable;
using WB.Core.SharedKernels.DataCollection.Repositories;
using WB.Core.SharedKernels.DataCollection.WebApi;
using WB.Core.SharedKernels.Enumerator.OfflineSync.Messages;
using WB.Core.SharedKernels.Enumerator.OfflineSync.Services;
using WB.Core.SharedKernels.Enumerator.Services.Infrastructure.Storage;

namespace WB.Core.BoundedContexts.Supervisor.Services.Implementation.OfflineSyncHandlers
{
    public class SupervisorInterviewUploadStateHandler : IHandleCommunicationMessage
    {
        private readonly IPlainStorage<SuperivsorReceivedPackageLogEntry, int> receivedPackagesLog;
        private readonly IAudioFileStorage audioFileStorage;
        private readonly IImageFileStorage imageFileStorage;

        public SupervisorInterviewUploadStateHandler(
                IPlainStorage<SuperivsorReceivedPackageLogEntry, int> receivedPackagesLog,
                IAudioFileStorage audioFileStorage, IImageFileStorage imageFileStorage)
        {
            this.receivedPackagesLog = receivedPackagesLog;
            this.audioFileStorage = audioFileStorage;
            this.imageFileStorage = imageFileStorage;
        }

        public void Register(IRequestHandler requestHandler)
        {
            requestHandler.RegisterHandler<GetInterviewUploadStateRequest, GetInterviewUploadStateResponse>(GetInterviewUploadState);
        }

        public async Task<GetInterviewUploadStateResponse> GetInterviewUploadState(GetInterviewUploadStateRequest request)
        {
            var existingReceivedPackageLog = this.receivedPackagesLog.Where(
                x => x.FirstEventId == request.Check.FirstEventId &&
                     x.FirstEventTimestamp == request.Check.FirstEventTimeStamp &&
                     x.LastEventId == request.Check.LastEventId &&
                     x.LastEventTimestamp == request.Check.LastEventTimeStamp).Count;

            var audioFiles = await this.audioFileStorage.GetBinaryFilesForInterview(request.InterviewId);
            var audioNames = audioFiles.Select(bf => bf.FileName).ToHashSet();
            var images = await this.imageFileStorage.GetBinaryFilesForInterview(request.InterviewId);
            var imagesNames = images.Select(bf => bf.FileName).ToHashSet();

            return new GetInterviewUploadStateResponse
            {
                InterviewId = request.InterviewId,
                UploadState = new InterviewUploadState
                {
                    IsEventsUploaded = existingReceivedPackageLog > 0,
                    AudioFilesNames = audioNames,
                    ImagesFilesNames = imagesNames
                }
            };
        }
    }
}
