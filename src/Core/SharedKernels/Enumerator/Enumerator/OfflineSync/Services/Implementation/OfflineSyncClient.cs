using System;
using System.Threading.Tasks;
using WB.Core.SharedKernels.DataCollection.WebApi;
using WB.Core.SharedKernels.Enumerator.OfflineSync.Messages;

namespace WB.Core.SharedKernels.Enumerator.OfflineSync.Services.Implementation
{
    public class OfflineSyncClient : IOfflineSyncClient
    {
        private readonly INearbyCommunicator communicator;
        private readonly INearbyConnection nearbyConnection;

        public OfflineSyncClient(INearbyCommunicator communicator, INearbyConnection nearbyConnection)
        {
            this.communicator = communicator;
            this.nearbyConnection = nearbyConnection;
        }

        public Task<GetQuestionnaireListResponse> GetQuestionnaireList(string endpoint,
            IProgress<CommunicationProgress> progress = null)
        {
            return this.communicator.SendAsync<GetQuestionnaireListRequest, GetQuestionnaireListResponse>(this.nearbyConnection,
                endpoint, new GetQuestionnaireListRequest(), progress);
        }

        public Task<OkResponse> PostInterviewAsync(string endpoint, PostInterviewRequest package,
            IProgress<CommunicationProgress> progress = null)
        {
            return this.communicator.SendAsync<PostInterviewRequest, OkResponse>(this.nearbyConnection,
                endpoint, package, progress);
        }

        public Task<OkResponse> PostInterviewImageAsync(string endpoint, PostInterviewImageRequest postInterviewImageRequest,
            IProgress<CommunicationProgress> progress = null)
        {
            return this.communicator.SendAsync<PostInterviewImageRequest, OkResponse>(this.nearbyConnection,
                endpoint, postInterviewImageRequest, progress);
        }

        public Task<OkResponse> PostInterviewAudioAsync(string endpoint, PostInterviewAudioRequest postInterviewAudioRequest,
            IProgress<CommunicationProgress> progress = null)
        {
            return this.communicator.SendAsync<PostInterviewAudioRequest, OkResponse>(this.nearbyConnection,
                endpoint, postInterviewAudioRequest, progress);
        }
    }
}
