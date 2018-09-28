using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using WB.Core.GenericSubdomains.Portable;
using WB.Core.GenericSubdomains.Portable.Implementation;
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
            this.nearbyConnection.RemoteEndpoints.CollectionChanged += (sender, args) =>
            {
                Endpoint = this.nearbyConnection.RemoteEndpoints.FirstOrDefault()?.Enpoint;
            };
        }

        public static string Endpoint { get; set; }

        public Task<TResponse> SendAsync<TRequest, TResponse>(TRequest request, CancellationToken cancellationToken,
            IProgress<TransferProgress> progress = null)
            where TRequest : ICommunicationMessage
            where TResponse : ICommunicationMessage
        {
            return this.communicator.SendAsync<TRequest, TResponse>(this.nearbyConnection, Endpoint, request, progress,
                cancellationToken);
        }

        public async Task SendAsync<TRequest>(TRequest request, CancellationToken cancellationToken,
            IProgress<TransferProgress> progress = null)
            where TRequest : ICommunicationMessage
        {
            await this.communicator.SendAsync<TRequest, OkResponse>(this.nearbyConnection, Endpoint, request, progress,
                cancellationToken);
        }

        public async Task<TResponse> SendChunkedAsync<TRequest, TResponse>(IChunkedByteArrayRequest request, CancellationToken cancellationToken,
            IProgress<TransferProgress> progress = null)
            where TRequest : IChunkedByteArrayRequest
            where TResponse : class, IChunkedByteArrayResponse
        {
            request.Skip = 0;
            request.Maximum = 1 * 1024 * 1024 / 4;

            byte[] content = null;
            TResponse response = null;

            var eta = new EtaTransferRate();

            // reporting back full data, not chunks info
            IProgress<TransferProgress> overallProgress = new Progress<TransferProgress>(t =>
            {
                t.TotalBytesToReceive = response?.Total;
                eta.AddProgress(t.BytesReceived, t.TotalBytesToReceive);
                t.Speed = eta.AverageSpeed;
                t.Eta = eta.ETA;
                
                t.BytesReceived = request.Skip + t.BytesReceived;
                
                if (t.TotalBytesToReceive != null)
                {
                    t.ProgressPercentage = request.Skip.PercentOf(t.TotalBytesToReceive.Value);
                }

                progress?.Report(t);
            });

            do
            {
                response = await this.communicator.SendAsync<TRequest, TResponse>(this.nearbyConnection, 
                        Endpoint, (TRequest)request, overallProgress, cancellationToken)
                    .ConfigureAwait(false);

                if (response.Content == null)
                {
                    return response;
                }

                content = content ?? new byte[response.Total];
                
                Array.Copy(response.Content, 0, content, response.Skipped, response.Length);

                request.Skip = response.Skipped + response.Length;
                overallProgress.Report(new TransferProgress
                {
                    BytesReceived = request.Skip,
                    TotalBytesToReceive = response.Total,
                    ProgressPercentage = request.Skip.PercentOf(response.Total)
                });
            } while (request.Skip < response.Total);

            response.Content = content;
           
            return response;
        }
    }
}
