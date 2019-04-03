using System;
using System.Collections.ObjectModel;
using System.IO;
using System.Threading;
using System.Threading.Tasks;
using Moq;
using NUnit.Framework;
using WB.Core.GenericSubdomains.Portable.Implementation;
using WB.Core.Infrastructure.FileSystem;
using WB.Core.SharedKernels.Enumerator.OfflineSync.Entities;
using WB.Core.SharedKernels.Enumerator.OfflineSync.Messages;
using WB.Core.SharedKernels.Enumerator.OfflineSync.Services;
using WB.Core.SharedKernels.Enumerator.OfflineSync.Services.Implementation;

namespace WB.Tests.Unit.Infrastructure.OfflineSync
{
    [TestFixture]
    public class OfflineSynchronizationChunkedSendTest
    {
        [Test]
        public async Task should_send_data_in_chunks_and_receive_whole_array()
        {
            var content = new byte[]
            {
                1, 2, 3, 4, 5,
                6, 7, 8, 9, 10,
                11, 12, 13, 14, 15,
                16, 17
            };

            // setting fake communicator that will return `conent` with chunk in maximum 5 bytes
            var communicator = new FakeCommunicator(content, 5);
            var connection = new Mock<INearbyConnection>();
            connection.Setup(c => c.RemoteEndpoints).Returns(new ObservableCollection<RemoteEndpoint>());

            var offlineSync = new OfflineSyncClient(communicator, connection.Object);

            // Act
            var result = await offlineSync.SendChunkedAsync<Request, Response>(new Request(), CancellationToken.None);

            // Assert
            Assert.That(result.Content, Is.EqualTo(content));
            Assert.That(communicator.CallsCount, Is.EqualTo(4));
        }

        private class Request : IChunkedByteArrayRequest
        {
            public long Maximum { get; set; }
            public long Skip { get; set; }
        }

        private class Response : IChunkedByteArrayResponse
        {
            public byte[] Content { get; set; }
            public long Skipped { get; set; }
            public int Length { get; set; }
            public long Total { get; set; }
        }

        private class FakeCommunicator : INearbyCommunicator
        {
            private readonly byte[] content;
            private readonly int limit;
            public long CallsCount = 0;

            public FakeCommunicator(byte[] content, int limit)
            {
                this.content = content;
                this.limit = limit;
            }

            public Task<TResponse> SendAsync<TRequest, TResponse>(
                INearbyConnection connection, string endpoint,
                TRequest message, IProgress<TransferProgress> progress,
                CancellationToken cancellationToken)
                where TRequest : ICommunicationMessage
                where TResponse : ICommunicationMessage
            {
                if (message is IChunkedByteArrayRequest request)
                {
                    var bytes = new MemoryStream(content).ReadExactly(request.Skip, Math.Min(limit, request.Maximum));
                    var response = new Response
                    {
                        Content = bytes,
                        Skipped = request.Skip,
                        Length = bytes.Length,
                        Total = content.Length
                    };

                    CallsCount++;
                    return Task.FromResult((TResponse)(ICommunicationMessage)response);
                }

                return Task.FromResult(default(TResponse));
            }

            public Task ReceivePayloadAsync(INearbyConnection connection, string endpoint, IPayload payload)
            {
                throw new NotImplementedException();
            }

            public void ReceivePayloadTransferUpdate(INearbyConnection connection, string endpoint, NearbyPayloadTransferUpdate update)
            {
                throw new NotImplementedException();
            }

            public IObservable<IncomingDataInfo> IncomingInfo { get; }
        }
    }
}
