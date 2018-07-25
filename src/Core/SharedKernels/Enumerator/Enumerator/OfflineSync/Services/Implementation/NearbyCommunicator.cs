using System;
using System.Collections.Concurrent;
using System.Diagnostics;
using System.Reactive.Linq;
using System.Reactive.Subjects;
using System.Threading;
using System.Threading.Tasks;
using WB.Core.GenericSubdomains.Portable;
using WB.Core.GenericSubdomains.Portable.Implementation;
using WB.Core.GenericSubdomains.Portable.Services;
using WB.Core.SharedKernels.Enumerator.OfflineSync.Entities;
using WB.Core.SharedKernels.Enumerator.OfflineSync.Messages;
using WB.Core.SharedKernels.Enumerator.Utils;

namespace WB.Core.SharedKernels.Enumerator.OfflineSync.Services.Implementation
{
    public class NearbyCommunicator : INearbyCommunicator
    {
        public static TimeSpan MessageAwaitingTimeout = TimeSpan.FromSeconds(30);

        private readonly IPayloadProvider payloadProvider;
        private readonly IPayloadSerializer payloadSerializer;
        private readonly IConnectionsApiLimits connectionsApiLimits;
        private readonly ILogger logger;

        /// in all dictionaries below:
        ///     long - is the payload ID provided by Nearby runtime
        ///     Guid - is our on correlation id
        private readonly ConcurrentDictionary<long, PayloadHeader> headers = new ConcurrentDictionary<long, PayloadHeader>();
        private readonly ConcurrentDictionary<Guid, TaskCompletionSourceWithProgress> pending = new ConcurrentDictionary<Guid, TaskCompletionSourceWithProgress>();

        private readonly ConcurrentDictionary<long, IPayload> incomingPayloads = new ConcurrentDictionary<long, IPayload>();
        private readonly ConcurrentDictionary<long, IPayload> outgoingPayloads = new ConcurrentDictionary<long, IPayload>();

        private readonly IRequestHandler requestHandler;

        private readonly Subject<IncomingDataInfo> incomingDataInfo = new Subject<IncomingDataInfo>();

        public NearbyCommunicator(IRequestHandler requestHandler,
            IPayloadProvider payloadProvider,
            IPayloadSerializer payloadSerializer,
            IConnectionsApiLimits connectionsApiLimits,
            ILogger logger)
        {
            this.requestHandler = requestHandler;
            this.payloadProvider = payloadProvider;
            this.payloadSerializer = payloadSerializer;
            this.connectionsApiLimits = connectionsApiLimits;
            this.logger = logger;
            IncomingInfo = incomingDataInfo.AsObservable();
        }

        /// <summary>
        /// Subscribe for all incoming packages information
        /// </summary>
        public IObservable<IncomingDataInfo> IncomingInfo { get; }

        /// <summary>
        /// Sending TRequest payload to remote endpoint and awaiting for TResponse response
        /// </summary>
        /// <remarks>
        /// Each communication occure is two step process.
        ///     1. Sending small <see cref="PayloadHeader">`header`</see> package. That describe intent of sender, message type and size
        ///         a. If payload message is smaller then certain connections api limit, then embed payload in header
        ///         b. if payload message is bigger then connection api lmimt, then:
        ///             - Sending payload wrapped in internal <see cref="PayloadContent">NearbyPayload</see> wrapper
        ///
        /// Same steps occure in response.
        ///
        /// So sending one message generate following data flow, is case when all payloads bigger then limit:
        ///
        ///     A -- header  --> B
        ///     A -- payload --> B
        ///     B -- header  --> A
        ///     B -- payload --> B
        ///
        /// If fit in message size limit
        ///
        ///     A -- header with payload --> B
        ///     B -- header with payload --> A
        ///
        /// In Nearby we use two types of messages: BYTES and STREAM
        /// BYTES used for info/header messages
        /// STREAM for message.
        ///
        /// </remarks>
        /// <typeparam name="TRequest">ICommunicationMessage request</typeparam>
        /// <typeparam name="TResponse">ICommunicationMessage response</typeparam>
        /// <param name="connection">Connection established with Nearby</param>
        /// <param name="endpoint">Remote destination endpoint</param>
        /// <param name="message">Request to send</param>
        /// <param name="progress">Progress track</param>
        /// <param name="cancellationToken">Cancellation token</param>
        /// <returns></returns>
        public async Task<TResponse> SendAsync<TRequest, TResponse>(
            INearbyConnection connection, string endpoint,
            TRequest message, IProgress<TransferProgress> progress, CancellationToken cancellationToken)
            where TRequest : ICommunicationMessage
            where TResponse : ICommunicationMessage
        {
            Package payload = null;
         
            try
            {
                payload = await PreparePayload(endpoint, Guid.NewGuid(), message, true).ConfigureAwait(false);
                var tsc = new TaskCompletionSourceWithProgress(progress, cancellationToken);
                pending.TryAdd(payload.CorrelationId, tsc);

                logger.Verbose($"[{connection.GetEndpointName(endpoint) ?? endpoint}] #{payload.CorrelationId} - {typeof(TRequest).Name} => {typeof(TResponse).Name}");

                await SendOverWireAsync(connection, endpoint, payload);
                var response = await tsc.Task.ConfigureAwait(false);

                logger.Verbose($"[{connection.GetEndpointName(endpoint) ?? endpoint}] #{payload.CorrelationId} - {typeof(TRequest).Name} => {response.GetType().Name}");

                switch (response)
                {
                    case FailedResponse fail:
                        throw new Exception
                        {
                            Data =
                            {
                                ["error"] = fail.Error,
                                ["failedPayload"] = fail.FailedPayload,
                                ["errorMessage"] = fail.ErrorMessage,
                            }
                        }; 
                    case TResponse result:
                        return result;
                    default:
                        throw new Exception(
                            $"Unexpected return type. Expected: {typeof(TResponse).FullName}. Gor: {response.GetType().FullName}");
                }
            }
            catch (Exception inner)
            {
                this.logger.Verbose("Got exception. " + inner);
                if (payload != null)
                {
                    pending.TryRemove(payload.CorrelationId, out _);
                }

                if (inner is OperationCanceledException) throw;

                throw new CommunicationException(inner, connection, endpoint, message, typeof(TResponse));
            }
        }

        public async Task RecievePayloadAsync(INearbyConnection nearbyConnection, string endpoint, IPayload payload)
        {
            incomingPayloads.GetOrAdd(payload.Id, payload);

            switch (payload.Type)
            {
                case PayloadType.Bytes:
                    var header = await payloadSerializer.FromPayloadAsync<PayloadHeader>(payload.Bytes);
                    headers.TryAdd(header.PayloadId, header);
                    incomingDataInfo.OnNext(new IncomingDataInfo
                    {
                        Type = header.PayloadType,
                        Endpoint = endpoint,
                        Name = header.Name,
                        BytesTransfered = 0,
                        BytesPerSecond = 0,
                        TotalBytes = header.Size,
                        FlowDirection = DataFlowDirection.In,
                        IsCompleted = false
                    });
                    
                    if (header.PayloadContent != null)
                    {
                        await HandlePayloadContent(nearbyConnection, endpoint, header.PayloadContent);
                    }

                    break;
                case PayloadType.Stream:
                    this.logger.Verbose($"Got stream");
                    await payload.ReadStreamAsync();
                    break;
                case PayloadType.File:
                    break;
                default:
                    throw new ArgumentOutOfRangeException();
            }
        }

        public async void RecievePayloadTransferUpdate(INearbyConnection connection, string endpoint, NearbyPayloadTransferUpdate update)
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

            PayloadHeader header;

            switch (update.Status)
            {
                case TransferStatus.Success:
                    if (payload.Type == PayloadType.Bytes) return;

                    if (isIncoming)
                    {
                        var bytes = await payload.BytesFromStream;
                        var payloadContent = await payloadSerializer.FromPayloadAsync<PayloadContent>(bytes);
                        await HandlePayloadContent(connection, endpoint, payloadContent);
                        logger.Verbose($"[{connection.GetEndpointName(endpoint) ?? endpoint}] #{payloadContent.CorrelationId} Incoming message - {payloadContent.Payload.GetType()}");

                        if (headers.TryGetValue(update.Id, out header))
                        {
                            incomingDataInfo.OnNext(new IncomingDataInfo
                            {
                                Type = header.PayloadType,
                                Endpoint = endpoint,
                                Name = header.Name,
                                BytesTransfered = header.Size,
                                BytesPerSecond = 0,
                                TotalBytes = header.Size,
                                FlowDirection = DataFlowDirection.In,
                                IsCompleted = false
                            });
                        }
                    }

                    break;
                case TransferStatus.Failure:
                    // TODO: revise what else need to be done in case of TransferFailure
                    if (headers.TryRemove(update.Id, out header))
                    {
                        incomingDataInfo.OnNext(new IncomingDataInfo
                        {
                            Type = header.PayloadType,
                            Endpoint = endpoint,
                            Name = header.Name,
                            BytesTransfered = 0,
                            BytesPerSecond = 0,
                            TotalBytes = header.Size,
                            FlowDirection = isIncoming ? DataFlowDirection.In : DataFlowDirection.Out,
                            IsCompleted = true
                        });

                        if (pending.TryRemove(header.CorrelationId, out var failure))
                            failure.SetCanceled();
                    }

                    break;
                case TransferStatus.InProgress:
                    {
                        if (headers.TryGetValue(update.Id, out header))
                        {
                            if (pending.TryGetValue(header.CorrelationId, out var pendingValue))
                            {
                                pendingValue.Debounce();
                                pendingValue.UpdateProgress(update.BytesTransferred, header.Size);
                            }
                            
                            incomingDataInfo.OnNext(new IncomingDataInfo
                            {
                                Type = header.PayloadType,
                                Endpoint = endpoint,
                                Name = header.Name,
                                BytesTransfered = update.BytesTransferred,
                                BytesPerSecond = pendingValue.ProgressData?.Speed ?? 0,
                                TotalBytes = header.Size,
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

        private async Task HandlePayloadContent(INearbyConnection nearbyConnection, string endpoint, PayloadContent payloadContent)
        {
            if (payloadContent.IsRequest)
            {
                ICommunicationMessage response;
                try
                {
                    response = await requestHandler.Handle(payloadContent.Payload);
                }
                catch (Exception e)
                {
                    var errorResponse = await PreparePayload(endpoint, payloadContent.CorrelationId, 
                        new FailedResponse
                        {
                            ErrorMessage = e.Message,
                            Error = e.ToString(),
                            Endpoint = endpoint
                        }, false, e.Message);

                    await SendOverWireAsync(nearbyConnection, endpoint, errorResponse);
                    return;
                }

                var responsePayload = await PreparePayload(endpoint, payloadContent.CorrelationId, response, false);
                await SendOverWireAsync(nearbyConnection, endpoint, responsePayload);
            }
            else
            {
                if (pending.TryGetValue(payloadContent.CorrelationId, out var tsc)) tsc.SetResult(payloadContent.Payload);
            }
        }

        private async Task SendOverWireAsync(INearbyConnection nearbyConnection, string endpoint, Package package)
        {
            await SendOverWireInternalAsync(package.Header);
            CommunicationSession.Current.RequestsTotal += 1;

            if (package.HeaderPayload.PayloadContent == null)
            {
                await SendOverWireInternalAsync(package.Content);
                CommunicationSession.Current.RequestsTotal += 1;
            }

            async Task SendOverWireInternalAsync(IPayload payload)
            {
                outgoingPayloads.AddOrUpdate(payload.Id, payload, (id, p) => payload);

                await nearbyConnection.SendPayloadAsync(endpoint, payload);
            }
        }
        

        private async Task<Package> PreparePayload(string endpoint, Guid correlationId, ICommunicationMessage payload, bool isRequest, string errorMessage = null)
        {
            var sw = Stopwatch.StartNew();

            var package = new Package();
            package.CorrelationId = correlationId;
            package.PayloadContent = new PayloadContent(correlationId, payload, isRequest);
            package.PayloadContentBytes = await payloadSerializer.ToPayloadAsync(package.PayloadContent);
            package.Content = payloadProvider.AsStream(package.PayloadContentBytes, endpoint);

            package.HeaderPayload = new PayloadHeader(package.PayloadContent, package.PayloadContentBytes, package.Content);

            if (errorMessage != null)
            {
                package.HeaderPayload.IsSuccess = false;
                package.HeaderPayload.Message = errorMessage;
            }

            package.HeaderBytes = await payloadSerializer.ToPayloadAsync(package.HeaderPayload);
            package.HeaderPayload.PayloadType = payload.GetType().FullName;
            headers.TryAdd(package.Content.Id, package.HeaderPayload);

            if (package.CanFitIntoHeader(connectionsApiLimits.MaxBytesLength))
            {
                package.HeaderPayload.PayloadContent = package.PayloadContent;

                package.HeaderBytes = await payloadSerializer.ToPayloadAsync(package.HeaderPayload);
            }

            package.Header = payloadProvider.AsBytes(package.HeaderBytes, endpoint);
            sw.Stop();
            logger.Verbose("Took " + sw.ElapsedMilliseconds + "ms");
            return package;
        }

        private class Package
        {
            public Guid CorrelationId { get; set; }
            public IPayload Content { get; set; }
            public IPayload Header { get; set; }
            public PayloadContent PayloadContent { get; set; }
            public byte[] PayloadContentBytes { get; set; }
            public PayloadHeader HeaderPayload { get; set; }
            public byte[] HeaderBytes { get; set; }

            public bool CanFitIntoHeader(long limit, long boundaryDelta = 1024)
            {
                return boundaryDelta + PayloadContentBytes.LongLength + HeaderBytes.Length < limit;
            }
        }

        private class PayloadContent
        {
            public PayloadContent(Guid correlationId, ICommunicationMessage payload, bool isRequest)
            {
                CorrelationId = correlationId;
                Payload = payload;
                IsRequest = isRequest;
                SessionId = CommunicationSession.Current.SessionId;
            }

            public Guid CorrelationId { get; }
            public Guid SessionId { get; }

            public bool IsRequest { get; }

            public ICommunicationMessage Payload { get; }
        }

        private class PayloadHeader
        {
            // ReSharper disable once UnusedMember.Local - used by Newtonsoft.Json
            public PayloadHeader()
            {
            }

            public PayloadHeader(PayloadContent payloadContent, byte[] payloadData, IPayload payload)
            {
                CorrelationId = payloadContent.CorrelationId;
                SessionId = payloadContent.SessionId;
                Size = payloadData.LongLength;
                PayloadId = payload.Id;
            }

            public long Size { get; set; }
            public Guid CorrelationId { get; set; }
            public Guid SessionId { get; set; }
            public string PayloadType { get; set; }
            public string Name { get; set; }
            public long PayloadId { get; set; }
            public string Message { get; set; }
            public PayloadContent PayloadContent { get; set; }
            public bool IsSuccess { get; set; } = true;
        }

        private struct TaskCompletionSourceWithProgress
        {
            private Timer timer;
            private readonly Stopwatch sw;
            private readonly EtaTransferRate etaHelper;
            public TransferProgress ProgressData { get; }
            private bool isCompleted;

            public TaskCompletionSourceWithProgress(IProgress<TransferProgress> progress,
                CancellationToken cancellationToken) : this()
            {
                TaskCompletionSource = new TaskCompletionSource<ICommunicationMessage>();
                Progress = progress;
                ProgressData = new TransferProgress();
                sw = Stopwatch.StartNew();
                etaHelper = new EtaTransferRate();
                Debounce();
                isCompleted = false;

                cancellationToken.Register(SetCanceled);
            }

            public void Debounce()
            {
                sw.Restart();
                timer?.Dispose();
                timer = new Timer(SetCanceled, null, (int)MessageAwaitingTimeout.TotalMilliseconds,
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

                ProgressData.TotalBytesToReceive = totalBytes;
                ProgressData.BytesReceived = sendBytes;
                ProgressData.Eta = etaHelper.ETA;
                ProgressData.Speed = etaHelper.AverageSpeed;//.Bytes().ToString("0.00");

                Progress?.Report(ProgressData);
            }

            private TaskCompletionSource<ICommunicationMessage> TaskCompletionSource { get; }

            public IProgress<TransferProgress> Progress { get; }

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
