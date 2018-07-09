using System;
using System.Collections.Concurrent;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.Reactive.Linq;
using System.Reactive.Subjects;
using System.Threading;
using System.Threading.Tasks;
using Humanizer;
using WB.Core.GenericSubdomains.Portable;
using WB.Core.SharedKernels.Enumerator.OfflineSync.Entities;
using WB.Core.SharedKernels.Enumerator.OfflineSync.Messages;
using WB.Core.SharedKernels.Enumerator.Utils;

namespace WB.Core.SharedKernels.Enumerator.OfflineSync.Services.Implementation
{
    public class NearbyCommunicator : INearbyCommunicator
    {
        public static TimeSpan MessageAwaitingTimeout = TimeSpan.FromSeconds(30);

        private readonly ConcurrentDictionary<long, IPayload> incomingPayloads =
            new ConcurrentDictionary<long, IPayload>();

        private readonly ConcurrentDictionary<long, IPayload> outgoingPayloads =
            new ConcurrentDictionary<long, IPayload>();

        private readonly IPayloadProvider payloadProvider;
        private readonly IPayloadSerializer payloadSerializer;
        private readonly ConcurrentDictionary<long, IncomingPayloadInfo> payloadsInfo = new ConcurrentDictionary<long, IncomingPayloadInfo>();
        private readonly ConcurrentDictionary<Guid, TaskCompletionSourceWithProgress> pending = new ConcurrentDictionary<Guid, TaskCompletionSourceWithProgress>();

        private readonly IRequestHandler requestHandler;

        private readonly Subject<IncomingDataInfo> incomingDataInfo = new Subject<IncomingDataInfo>();
        
        public NearbyCommunicator(IRequestHandler requestHandler,
            IPayloadProvider payloadProvider, IPayloadSerializer payloadSerializer)
        {
            this.requestHandler = requestHandler;
            this.payloadProvider = payloadProvider;
            this.payloadSerializer = payloadSerializer;
            IncomingInfo = incomingDataInfo.AsObservable();
        }

        public IObservable<IncomingDataInfo> IncomingInfo { get; }

        public async Task<TResponse> SendAsync<TRequest, TResponse>(INearbyConnection connection, string endpoint,
            TRequest message, IProgress<CommunicationProgress> progress = null)
            where TRequest : ICommunicationMessage
            where TResponse : ICommunicationMessage
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

                var response = await tsc.Task;

                switch (response)
                {
                    case FailedResponse fail:
                        throw new Exception(); // todo temp
                    case TResponse result:
                        return result;
                    default:
                        throw new Exception(
                            $"Unexpected return type. Expected: {typeof(TResponse).FullName}. Gor: {response.GetType().FullName}");
                }
            }
            catch (TaskCanceledException)
            {
                pending.TryRemove(payloads.id, out _);
                throw;
            }
        }

        public async Task RecievePayloadAsync(INearbyConnection nearbyConnection, string endpointId, IPayload payload)
        {
            incomingPayloads.GetOrAdd(payload.Id, payload);

            switch (payload.Type)
            {
                case PayloadType.Bytes:
                    var info = payloadSerializer.FromPayload<IncomingPayloadInfo>(payload.Bytes);

                    Debug("RECEIVE", false, endpointId, payload.Id, payload.Type, $"Info: {info.NearbyPayloadId}");
                    payloadsInfo.TryAdd(info.PayloadId, info);

                    incomingDataInfo.OnNext(new IncomingDataInfo
                    {
                        Type = info.PayloadType,
                        Endpoint = endpointId,
                        Name = info.Name,
                        BytesTransfered = 0,
                        BytesPerSecond = 0,
                        TotalBytes = info.Size,
                        FlowDirection = DataFlowDirection.In,
                        IsCompleted = false
                    });

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
            var isIncoming = false;
            if (incomingPayloads.TryGetValue(update.Id, out var payload))
            {
                isIncoming = true;
                if (update.Status != TransferStatus.InProgress) incomingPayloads.TryRemove(update.Id, out _);
            }
            else if (outgoingPayloads.TryGetValue(update.Id, out payload))
            {
                if (update.Status != TransferStatus.InProgress) outgoingPayloads.TryRemove(update.Id, out _);
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
                    if (payload.Type == PayloadType.Bytes) return;

                    if (isIncoming)
                    {
                        var bytes = await payload.BytesFromStream;

                        var message = payloadSerializer.FromPayload<NearbyPayload>(bytes);

                        if (message.IsRequest)
                        {
                            ICommunicationMessage response;
                            try
                            {
                                response = await requestHandler.Handle(message.Payload);
                            }
                            catch (Exception e)
                            {
                                //TODO: HANDLE ERROR SEND AS RESPONSE IN INFO
                                var errorResponse = PreparePayload(message.Id, new FailedResponse(), false, e.Message);

                                await SendOverWire(nearbyConnection, endpoint, errorResponse.info);
                                throw;
                            }

                            var responsePayload = PreparePayload(message.Id, response, false);

                            await SendOverWire(nearbyConnection, endpoint, responsePayload.info);
                            await SendOverWire(nearbyConnection, endpoint, responsePayload.message);
                        }
                        else
                        {
                            if (pending.TryGetValue(message.Id, out var tsc)) tsc.SetResult(message.Payload);
                        }
                    }

                    if (payloadsInfo.TryGetValue(update.Id, out var payloadInfo))
                    {
                        incomingDataInfo.OnNext(new IncomingDataInfo
                        {
                            Type = payloadInfo.PayloadType,
                            Endpoint = endpoint,
                            Name = payloadInfo.Name,
                            BytesTransfered = payloadInfo.Size,
                            BytesPerSecond = 0,
                            TotalBytes = payloadInfo.Size,
                            FlowDirection = isIncoming ? DataFlowDirection.In : DataFlowDirection.Out,
                            IsCompleted = false
                        });
                    }

                    break;
                case TransferStatus.Failure:
                    // TODO: revise what else need to be done in case of TransferFailure
                    if (payloadsInfo.TryRemove(update.Id, out var failureInfo))
                    {
                        incomingDataInfo.OnNext(new IncomingDataInfo
                        {
                            Type = failureInfo.PayloadType,
                            Endpoint = endpoint,
                            Name = failureInfo.Name,
                            BytesTransfered = 0,
                            BytesPerSecond = 0,
                            TotalBytes = failureInfo.Size,
                            FlowDirection = isIncoming ? DataFlowDirection.In : DataFlowDirection.Out,
                            IsCompleted = true
                        });

                        if (pending.TryRemove(failureInfo.NearbyPayloadId, out var failure))
                            failure.SetCanceled();

                    }

                    break;
                case TransferStatus.InProgress:
                {
                    if (payloadsInfo.TryGetValue(update.Id, out var info))
                    {
                        if (pending.TryGetValue(info.NearbyPayloadId, out var pendingValue))
                        {
                            pendingValue.Debounce();

                            pendingValue.UpdateProgress(update.BytesTransferred, info.Size);
                        }

                        incomingDataInfo.OnNext(new IncomingDataInfo
                        {
                            Type = info.PayloadType,
                            Endpoint = endpoint,
                            Name = info.Name,
                            BytesTransfered = update.BytesTransferred,
                            BytesPerSecond = 0,
                            TotalBytes = info.Size,
                            FlowDirection = isIncoming ? DataFlowDirection.In : DataFlowDirection.Out,
                            IsCompleted = false
                        });
                    }

                    break;
                }
                default:
                    throw new ArgumentOutOfRangeException();
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
                $"[{action,-11}] {(outgoing ? "====>" : "<====")} {endpoint}[{payloadId}] #{type.ToString()} {message}");
        }

        private (Guid id, IPayload message, IPayload info) PreparePayload(Guid correlationGuid,
            ICommunicationMessage payload, bool isRequest, string errorMessage = null)
        {
            var nearbyPayload = new NearbyPayload(correlationGuid, payload, isRequest);
            var payloadBytes = payloadSerializer.ToPayload(nearbyPayload);
            var outgoingPayload = payloadProvider.AsStream(payloadBytes);

            var info = new IncomingPayloadInfo(nearbyPayload, payloadBytes, outgoingPayload);

            if (errorMessage != null)
            {
                info.IsSuccess = false;
                info.Message = errorMessage;
            }

            info.PayloadType = payload.GetType().FullName;

            payloadsInfo.TryAdd(outgoingPayload.Id, info);

            var infoPayload = payloadProvider.AsBytes(payloadSerializer.ToPayload(info));

            return (nearbyPayload.Id, outgoingPayload, infoPayload);
        }

        private class NearbyPayload
        {
            public NearbyPayload(Guid id, ICommunicationMessage payload, bool isRequest)
            {
                Id = id;
                Payload = payload;
                IsRequest = isRequest;
            }

            public Guid Id { get; }

            public bool IsRequest { get; }

            public ICommunicationMessage Payload { get; }
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

            public long Size { get; }
            public Guid NearbyPayloadId { get; }
            public string PayloadType { get; set; }
            public string Name { get; set; }
            public long PayloadId { get; }
            public string Message { get; set; }

            public bool IsSuccess { get; set; } = true;
        }

        private struct TaskCompletionSourceWithProgress
        {
            private Timer timer;
            private readonly Stopwatch sw;
            private readonly EtaTransferRate etaHelper;
            private readonly CommunicationProgress progressData; // = new CommunicationProgress();
            private bool isCompleted;

            public TaskCompletionSourceWithProgress(IProgress<CommunicationProgress> progress) : this()
            {
                TaskCompletionSource = new TaskCompletionSource<ICommunicationMessage>();
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
                timer?.Dispose();
                timer = new Timer(SetCanceled, null, (int) MessageAwaitingTimeout.TotalMilliseconds,
                    Timeout.Infinite);
            }

            private void SetCanceled(object state)
            {
                if (isCompleted || sw.Elapsed < MessageAwaitingTimeout) return;

                TaskCompletionSource.TrySetCanceled();
                timer?.Dispose();
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

            private TaskCompletionSource<ICommunicationMessage> TaskCompletionSource { get; }

            public IProgress<CommunicationProgress> Progress { get; }

            public Task<ICommunicationMessage> Task => TaskCompletionSource.Task;

            public void SetCanceled()
            {
                SetCanceled(null);
                isCompleted = true;
            }

            public void SetResult(ICommunicationMessage data)
            {
                isCompleted = true;
                TaskCompletionSource.TrySetResult(data);
            }
        }
    }
}
