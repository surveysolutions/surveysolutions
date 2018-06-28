using System;
using System.Threading.Tasks;
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

        public Task<SendBigAmountOfDataResponse> SendBigData(string endpoint, byte[] data, IProgress<CommunicationProgress> progress = null)
        {
            return this.communicator.SendAsync<SendBigAmountOfDataRequest, SendBigAmountOfDataResponse>(
                this.nearbyConnection,
                endpoint, new SendBigAmountOfDataRequest { Data = data }, progress);
        }
    }
}
