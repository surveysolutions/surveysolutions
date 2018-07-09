using System;
using System.Threading;
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

        public static string Endpoint { get; set; }

        public Task<TResponse> SendAsync<TRequest, TResponse>(TRequest request, CancellationToken cancellationToken, IProgress<CommunicationProgress> progress = null) 
            where TRequest : ICommunicationMessage
            where TResponse : ICommunicationMessage
        {
            return this.communicator.SendAsync<TRequest, TResponse>(this.nearbyConnection, Endpoint, request, progress);
        }

        public async Task SendAsync<TRequest>(TRequest request, CancellationToken cancellationToken, IProgress<CommunicationProgress> progress = null)
            where TRequest : ICommunicationMessage
        {
            await this.communicator.SendAsync<TRequest, OkResponse>(this.nearbyConnection, Endpoint, request, progress);
        }
    }
}
