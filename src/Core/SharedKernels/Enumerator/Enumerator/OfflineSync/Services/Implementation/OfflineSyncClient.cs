using System;
using System.Linq;
using System.Net;
using System.Threading;
using System.Threading.Tasks;
using WB.Core.GenericSubdomains.Portable;
using WB.Core.GenericSubdomains.Portable.Implementation;
using WB.Core.Infrastructure.FileSystem;
using WB.Core.Infrastructure.HttpServices.HttpClient;
using WB.Core.SharedKernels.Enumerator.OfflineSync.Messages;

namespace WB.Core.SharedKernels.Enumerator.OfflineSync.Services.Implementation
{
    public class OfflineSyncClient : IOfflineSyncClient
    {
        private readonly INearbyCommunicator communicator;
        private readonly INearbyConnection nearbyConnection;
        private readonly IFileSystemAccessor fileSystemAccessor;
        public OfflineSyncClient(INearbyCommunicator communicator, INearbyConnection nearbyConnection,
            IFileSystemAccessor fileSystemAccessor)
        {
            this.communicator = communicator;
            this.nearbyConnection = nearbyConnection;
            this.fileSystemAccessor = fileSystemAccessor;

            this.nearbyConnection.RemoteEndpoints.CollectionChanged += (sender, args) =>
            {
                Endpoint = this.nearbyConnection.RemoteEndpoints.FirstOrDefault()?.Endpoint;
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

        public Task SendAsync<TRequest>(TRequest request, CancellationToken cancellationToken,
            IProgress<TransferProgress> progress = null)
            where TRequest : ICommunicationMessage
        {
            return this.communicator.SendAsync<TRequest, OkResponse>(this.nearbyConnection, Endpoint, request, progress,
                cancellationToken);
        }

        public async Task<TResponse> SendChunkedAsync<TRequest, TResponse>(IChunkedByteArrayRequest request,
            CancellationToken cancellationToken, IProgress<TransferProgress> progress = null)
            where TRequest : IChunkedByteArrayRequest
            where TResponse : class, IChunkedByteArrayResponse
        {
            request.Skip = 0;
            request.Maximum = 1024 * 512;

            byte[] content = null;
            TResponse response = null;
            byte[] responseHash = null;
            // reporting back full data, not chunks info
            IProgress<TransferProgress> overallProgress = new Progress<TransferProgress>(t =>
            {
                t.TotalBytesToReceive = response?.Total;
                
                t.BytesReceived = request.Skip + t.BytesReceived;

                if (t.TotalBytesToReceive != null)
                {
                    t.ProgressPercentage = t.BytesReceived.PercentOf(t.TotalBytesToReceive.Value);
                }

                progress?.Report(t);
            });

            do
            {
                response = await this.communicator.SendAsync<TRequest, TResponse>(
                    this.nearbyConnection, Endpoint, (TRequest)request, overallProgress, cancellationToken);

                if (response.Content == null)
                {
                    return response;
                }

                responseHash = response.ContentMD5;
                content ??= new byte[response.Total];

                Array.Copy(response.Content, 0, content, response.Skipped, response.Length);

                request.Skip = response.Skipped + response.Length;
            } while (request.Skip < response.Total);

            progress?.Report(new TransferProgress
            {
                BytesReceived = response.Total,
                TotalBytesToReceive = response.Total,
                ProgressPercentage = 100
            });

            if (!fileSystemAccessor.IsHashValid(content, responseHash))
            {
                throw new RestException("Received data checksum failed", HttpStatusCode.BadRequest);
            }

            response.Content = content;

            return response;
        }
    }
}
