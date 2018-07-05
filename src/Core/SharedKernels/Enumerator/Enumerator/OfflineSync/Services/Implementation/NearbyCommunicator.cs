using System;
using System.Collections.Concurrent;
using System.Diagnostics;
using System.Threading;
using System.Threading.Tasks;
using Humanizer;
using WB.Core.GenericSubdomains.Portable;
using WB.Core.SharedKernels.Enumerator.OfflineSync.Entities;

namespace WB.Core.SharedKernels.Enumerator.OfflineSync.Services.Implementation
{
    public class NearbyCommunicator : INearbyCommunicator
    {
        private readonly IPayloadSerializer payloadSerializer;
        private readonly IRequestHandler requestHandler;
        private readonly IPayloadProvider payloadProvider;

        private readonly ConcurrentDictionary<Guid, TaskCompletionSourceWithProgress> pending =
            new ConcurrentDictionary<Guid, TaskCompletionSourceWithProgress>();

        private readonly ConcurrentDictionary<long, IncomingPayloadInfo> payloadsInfo =
            new ConcurrentDictionary<long, IncomingPayloadInfo>();

        private readonly ConcurrentDictionary<long, IPayload> incomingPayloads =
            new ConcurrentDictionary<long, IPayload>();

        private readonly ConcurrentDictionary<long, IPayload> outgoingPayloads =
            new ConcurrentDictionary<long, IPayload>();

        public NearbyCommunicator(IRequestHandler requestHandler,
            IPayloadProvider payloadProvider, IPayloadSerializer payloadSerializer)
        {
            this.requestHandler = requestHandler;
            this.payloadProvider = payloadProvider;
            this.payloadSerializer = payloadSerializer;
        }

        private (Guid id, IPayload message, IPayload info) PreparePayload(Guid correlationGuid, byte[] dataToSend,
            bool isRequest, string errorMessage = null)
        {
            var nearbyPayload = new NearbyPayload(correlationGuid, dataToSend, isRequest);
            var payloadBytes = payloadSerializer.ToPayload(nearbyPayload);
            var outgoingPayload = this.payloadProvider.AsStream(payloadBytes);

            var info = new IncomingPayloadInfo(nearbyPayload, payloadBytes, outgoingPayload);
            if (errorMessage != null)
            {
                info.IsSuccess = false;
                info.Message = errorMessage;
            }

            this.payloadsInfo.TryAdd(outgoingPayload.Id, info);

            var infoPayload = this.payloadProvider.AsBytes(this.payloadSerializer.ToPayload(info));

            return (nearbyPayload.Id, outgoingPayload, infoPayload);
        }

        public async Task<TResponse> SendAsync<TRequest, TResponse>(INearbyConnection connection, string endpoint,
            TRequest message, IProgress<CommunicationProgress> progress)
        {
            var requestBytes = payloadSerializer.ToPayload(message);
            var responseBytes = await this.SendAsync(connection, endpoint, requestBytes, progress);
            return this.payloadSerializer.FromPayload<TResponse>(responseBytes);
        }

        private async Task<byte[]> SendAsync(INearbyConnection connection, string endpoint, byte[] message,
            IProgress<CommunicationProgress> progress = null)
        {
            var payloads = PreparePayload(Guid.NewGuid(), message, true);

            try
            {
                var tsc = new TaskCompletionSourceWithProgress(progress);
                pending.TryAdd(payloads.id, tsc);

                Debug("SEND.Message", true, endpoint, payloads.id, PayloadType.Bytes, "");
                // sending info about outgoing package
                await SendOverWire(connection, endpoint, payloads.info);

                // sending message package
                await SendOverWire(connection, endpoint, payloads.message);

                return await tsc.Task;
            }
            catch (TaskCanceledException)
            {
                pending.TryRemove(payloads.id, out _);
                throw;
            }
        }

        private Task SendOverWire(INearbyConnection nearbyConnection, string endpoint, IPayload payload)
        {
            outgoingPayloads.AddOrUpdate(payload.Id, payload, (id, p) => payload);
            Debug("SEND", true, endpoint, payload.Id, payload.Type, "Send over wire");
            return nearbyConnection.SendPayloadAsync(endpoint, payload);
        }

        [Conditional("DEBUG")]
        private void Debug(string action, bool outgoing, string endpoint, object payloadId, PayloadType type,
            string message)
        {
            System.Diagnostics.Debug.WriteLine(
                $"[{action,-11}] {(outgoing ? "====>" : "<====")} {endpoint}[{payloadId.ToString()}] #{type.ToString()} {message}");
        }

        public async Task RecievePayloadAsync(INearbyConnection nearbyConnection, string endpointId, IPayload payload)
        {
            incomingPayloads.GetOrAdd(payload.Id, payload);

            switch (payload.Type)
            {
                case PayloadType.Bytes:
                    var info = this.payloadSerializer.FromPayload<IncomingPayloadInfo>(payload.Bytes);
                    Debug("RECEIVE", false, endpointId, payload.Id, payload.Type, $"Info: {info.NearbyPayloadId}");
                    this.payloadsInfo.TryAdd(info.PayloadId, info);
                    break;
                case PayloadType.Stream:
                    Debug("RECEIVE", false, endpointId, payload.Id, payload.Type, $"Start reading");
                    await payload.ReadStreamAsync();
                    Debug("RECEIVE", false, endpointId, payload.Id, payload.Type, $"Done reading");
                    break;
            }
        }

        public async void RecievePayloadTransferUpdate(INearbyConnection nearbyConnection, string endpoint,
            NearbyPayloadTransferUpdate update)
        {
            bool isIncoming = false;
            if (incomingPayloads.TryGetValue(update.Id, out var payload))
            {
                isIncoming = true;
                if (update.Status != TransferStatus.InProgress)
                {
                    incomingPayloads.TryRemove(update.Id, out _);
                }
            }
            else if (outgoingPayloads.TryGetValue(update.Id, out payload))
            {
                if (update.Status != TransferStatus.InProgress)
                {
                    outgoingPayloads.TryRemove(update.Id, out _);
                }
            }
            else
            {
                throw new ApplicationException("Recieve payload transfer update before RecievePayload call");
            }

            Debug("UPDATE", isIncoming, endpoint, update.Id, payload.Type, update.Status.ToString());

            switch (update.Status)
            {
                case TransferStatus.Success:
                    // handle info package
                    if (payload.Type == PayloadType.Bytes)
                    {
                        //if (isIncoming == false) return; // ignore outgoing success status for BYTES info payload
                        return;
                    }

                    if (isIncoming)
                    {
                        var bytes = await payload.BytesFromStream;

                        var message = payloadSerializer.FromPayload<NearbyPayload>(bytes);

                        if (message.IsRequest)
                        {
                            byte[] response;
                            try
                            {
                                response = await requestHandler.Handle(message.Data);
                            }
                            catch (Exception e)
                            {
                                //TODO: HANDLE ERROR SEND AS RESPONSE IN INFO
                                var errorResponse = PreparePayload(message.Id, Array.Empty<byte>(), false, e.Message);

                                await SendOverWire(nearbyConnection, endpoint, errorResponse.info);
                                throw;
                            }

                            var responsePayload = PreparePayload(message.Id, response, false);

                            await SendOverWire(nearbyConnection, endpoint, responsePayload.info);
                            await SendOverWire(nearbyConnection, endpoint, responsePayload.message);
                        }
                        else
                        {
                            if (pending.TryGetValue(message.Id, out var tsc))
                            {
                                tsc.SetResult(message.Data);
                            }
                        }
                    }

                    break;
                case TransferStatus.Failure:
                    // TODO: revise what else need to be done in case of TransferFailure
                    if (this.payloadsInfo.TryRemove(update.Id, out var failureInfo))
                    {
                        if (this.pending.TryRemove(failureInfo.NearbyPayloadId, out var failure))
                        {
                            failure.SetCanceled();
                        }
                    }

                    break;
                case TransferStatus.InProgress:
                    if (this.payloadsInfo.TryGetValue(update.Id, out var info))
                    {
                        // TODO: NOTIFY ON PROGRESS
                        // notify app about pending stream data with data
                        // info.Size
                        // update.BytesTransferred

                        if (this.pending.TryGetValue(info.NearbyPayloadId, out var pendingValue))
                        {
                            pendingValue.Debounce();

                            pendingValue.UpdateProgress(update.BytesTransferred, info.Size);
                        }
                    }

                    break;
                default:
                    throw new ArgumentOutOfRangeException();
            }
        }

        private class NearbyPayload
        {
            public Guid Id { get; }

            public bool IsRequest { get; }

            public byte[] Data { get; }

            public NearbyPayload(Guid id, byte[] data, bool isRequest)
            {
                Id = id;

                Data = data;
                IsRequest = isRequest;
            }
        }

        private class IncomingPayloadInfo
        {
            // ReSharper disable once UnusedMember.Local - used by Newtonsoft json
            public IncomingPayloadInfo()
            {
            }

            public IncomingPayloadInfo(NearbyPayload nearbyPayload, byte[] payloadData, IPayload payload)
            {
                NearbyPayloadId = nearbyPayload.Id;
                Size = payloadData.LongLength;
                PayloadId = payload.Id;
            }

            public long Size { get; set; }
            public Guid NearbyPayloadId { get; set; }
            public long PayloadId { get; set; }
            public string Message { get; set; }

            public bool IsSuccess { get; set; } = true;
        }

        public static TimeSpan MessageAwaitingTimeout = TimeSpan.FromSeconds(30);

        private struct TaskCompletionSourceWithProgress
        {
            private Timer timer;
            private readonly Stopwatch sw;
            private readonly EtaTransferRate etaHelper;
            private readonly CommunicationProgress progressData; // = new CommunicationProgress();
            private bool isCompleted;

            public TaskCompletionSourceWithProgress(IProgress<CommunicationProgress> progress) : this()
            {
                TaskCompletionSource = new TaskCompletionSource<byte[]>();
                Progress = progress;
                progressData = new CommunicationProgress();
                sw = Stopwatch.StartNew();
                etaHelper = new EtaTransferRate();
                Debounce();
                isCompleted = false;
            }

            public void Debounce()
            {
                sw.Restart();
                this.timer?.Dispose();
                this.timer = new Timer(SetCanceled, null, (int) MessageAwaitingTimeout.TotalMilliseconds,
                    Timeout.Infinite);
            }

            private void SetCanceled(object state)
            {
                if (isCompleted || sw.Elapsed < MessageAwaitingTimeout)
                {
                    return;
                }

                TaskCompletionSource.TrySetCanceled();
                this.timer?.Dispose();
            }

            public void UpdateProgress(long sendBytes, long totalBytes)
            {
                etaHelper.AddProgress(sendBytes, totalBytes);

                progressData.TotalBytes = totalBytes;
                progressData.TransferedBytes = sendBytes;
                progressData.Eta = etaHelper.ETA;
                progressData.Speed = etaHelper.AverageSpeed.Bytes().ToString("0.00");

                Progress?.Report(progressData);
            }

            private TaskCompletionSource<byte[]> TaskCompletionSource { get; }

            public IProgress<CommunicationProgress> Progress { get; }

            public Task<byte[]> Task => TaskCompletionSource.Task;

            public void SetCanceled()
            {
                SetCanceled(null);
                isCompleted = true;
            }

            public void SetResult(byte[] data)
            {
                isCompleted = true;
                TaskCompletionSource.TrySetResult(data);
            }
        }
    }
}
