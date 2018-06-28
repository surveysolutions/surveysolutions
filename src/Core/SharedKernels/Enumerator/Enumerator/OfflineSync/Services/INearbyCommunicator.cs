using System;
using System.IO;
using System.Threading.Tasks;
using WB.Core.SharedKernels.Enumerator.OfflineSync.Entities;

namespace WB.Core.SharedKernels.Enumerator.OfflineSync.Services
{
    public struct CommunicationProgress
    {
        public long SendTotalBytes { get; set; }
        public long SendTransferedBytes { get; set; }

        public long RecievedTotalBytes { get; set; }
        public long RecievedTransferedBytes { get; set; }
    }

    public interface INearbyCommunicator
    {
        Task<TResponse> SendAsync<TRequest, TResponse>(INearbyConnection connection, 
            string endpoint, TRequest message, IProgress<CommunicationProgress> progress);

        Task RecievePayloadAsync(INearbyConnection nearbyConnection, string endpointId, IPayload payload);
        void RecievePayloadTransferUpdate(INearbyConnection nearbyConnection, string endpoint, NearbyPayloadTransferUpdate update);
    }
}
