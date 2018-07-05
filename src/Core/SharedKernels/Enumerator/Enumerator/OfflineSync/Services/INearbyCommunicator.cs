using System;
using System.Threading.Tasks;
using WB.Core.SharedKernels.Enumerator.OfflineSync.Entities;

namespace WB.Core.SharedKernels.Enumerator.OfflineSync.Services
{
    public interface INearbyCommunicator
    {
        Task<TResponse> SendAsync<TRequest, TResponse>(INearbyConnection connection,
            string endpoint, TRequest message, IProgress<CommunicationProgress> progress);

        Task RecievePayloadAsync(INearbyConnection nearbyConnection, string endpointId, IPayload payload);
        void RecievePayloadTransferUpdate(INearbyConnection nearbyConnection, string endpoint, NearbyPayloadTransferUpdate update);
    }
}
