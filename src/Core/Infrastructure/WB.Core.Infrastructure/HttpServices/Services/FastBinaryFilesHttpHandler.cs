using Polly;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Threading;
using System.Threading.Tasks;
using WB.Core.GenericSubdomains.Portable.Services;

namespace WB.Core.GenericSubdomains.Portable.Implementation.Services
{
    public class FastBinaryFilesHttpHandler : IFastBinaryFilesHttpHandler
    {
        private readonly IHttpClientFactory httpClientFactory;
        private readonly IRestServiceSettings restServiceSettings;
        private readonly IHttpStatistician httpStatistician;
        private readonly ILogger logger;

        private const int ChunkSize = 1024 * 1024; // 1Mb chunk

        public FastBinaryFilesHttpHandler(
            IHttpClientFactory httpClientFactory,
            IRestServiceSettings restServiceSettings,
            IHttpStatistician httpStatistician,
            ILogger logger)
        {
            this.httpClientFactory = httpClientFactory;
            this.restServiceSettings = restServiceSettings;
            this.httpStatistician = httpStatistician;
            this.logger = logger;
        }

        public static bool SupportRangeRequests(HttpResponseMessage response)
        {
            var responseLength = response.Content?.Headers.ContentLength;

            return (response.StatusCode == System.Net.HttpStatusCode.OK
                    || response.StatusCode == System.Net.HttpStatusCode.PartialContent)
                   && responseLength.HasValue
                   && responseLength < int.MaxValue && responseLength > ChunkSize
                   && response.Headers.AcceptRanges.Any();
        }

        public Task<byte[]> DownloadBinaryDataAsync(HttpClient http, HttpResponseMessage response,
            IProgress<TransferProgress> transferProgress,
            CancellationToken token)
        {
            if (SupportRangeRequests(response))
            {
                return DownloadAsyncInMultipleChunks(http, response, transferProgress, token);
            }

            return DownloadInSingleThreadAsync(response);
        }

        private Task<byte[]> DownloadInSingleThreadAsync(HttpResponseMessage responseMessage)
        {
            return responseMessage.Content.ReadAsByteArrayAsync();
        }

        private async Task<byte[]> DownloadAsyncInMultipleChunks(HttpClient http, HttpResponseMessage response,
            IProgress<TransferProgress> transferProgress, CancellationToken token)
        {
            var responseLength = response.Content.Headers.ContentLength ?? throw new ArgumentException("ContentLength required");
            var originalRequest = Clone(response.RequestMessage);

            // Canceling pending request, that return info about range support
            response.Dispose();
            http.CancelPendingRequests();

            // each chunk downloader will create own HttpClient
            http.Dispose();

            var result = new byte[responseLength];
            var chunksQueue = new BlockingCollection<Chunk>();

            var downloaders = new List<Task>();
            var downloaderIds = 0;
            var totalTime = new Stopwatch();
            long totalBytesReceived = 0;
            
            async Task<bool> ChunkDownloader()
            {
                var id = Interlocked.Increment(ref downloaderIds);

                var chunkTimer = new Stopwatch();
                var httpClient = httpClientFactory.CreateClient(this.httpStatistician);

                var progress = new TransferProgress { TotalBytesToReceive = responseLength };
                logger.Trace($"Downloader#{id}: Waiting for chunks");

                foreach (var chunk in chunksQueue.GetConsumingEnumerable())
                {
                    var rangeRequest = Clone(originalRequest);
                    rangeRequest.Headers.Range = new RangeHeaderValue(chunk.From, chunk.To);

                    chunkTimer.Restart();

                    var block = await Policy.Handle<Exception>()
                        .WaitAndRetryAsync(3, i => TimeSpan.FromSeconds(1), (e, t, i) =>
                        {
                            logger.Warn($"Downloader#{id}: Retry due to exception: {e.Message}", e);
                        })
                        .ExecuteAsync(() => httpClient.SendAsync(rangeRequest, HttpCompletionOption.ResponseHeadersRead, token))
                        .ConfigureAwait(false);

                    // fastest way to read whole response without allocating byte[] twice.
                    var buffer = await block.Content.ReadAsByteArrayAsync().ConfigureAwait(false);

                    // fill result array with acquired buffer data
                    Array.Copy(buffer, 0, result, chunk.From, chunk.Length);

                    chunkTimer.Stop();
                    totalBytesReceived = Interlocked.Add(ref totalBytesReceived, chunk.Length);
                    
                    logger.Trace($"Downloader#{id} - Chunk#{chunk.Index}" +
                                  $"[{chunk.From} - {chunk.To} = {chunk.Length}] - {totalBytesReceived}");
                    token.ThrowIfCancellationRequested();

                    if (transferProgress == null) continue;
                    progress.BytesReceived = totalBytesReceived;
                    progress.ProgressPercentage = progress.BytesReceived.PercentOf(responseLength);
                    progress.Speed = progress.BytesReceived / totalTime.Elapsed.TotalSeconds;

                    transferProgress.Report(progress);
                }

                logger.Trace($"Downloader#{id}: Completed all chunk downloads. Exiting");
                return true;
            }

            // starting N chunk download workers
            for (int i = 0; i < this.restServiceSettings.MaxDegreeOfParallelism; i++)
            {
                var task = Task.Run(ChunkDownloader, token);
                downloaders.Add(task);
            }

            totalTime.Restart();

            // setting initial chunk size and indexes
            int chunkSize = ChunkSize, last = 0, index = 0;

            while (last < responseLength - 1)
            {
                var start = last;

                // make sure that we will no set end chunk more then content length
                var end = Math.Min((int)responseLength - 1, start + chunkSize);
                last = end + 1;

                // adding new chunk will be blocked if queue is full
                chunksQueue.Add(new Chunk(index++, start, end), token);
            }

            chunksQueue.CompleteAdding();

            logger.Trace($"Competed adding, waiting for all");

            await Task.WhenAll(downloaders).ConfigureAwait(false);
            logger.Trace($"Download completed");

            if (transferProgress != null)
            {
                var progress = new TransferProgress { TotalBytesToReceive = responseLength };
                progress.BytesReceived = totalBytesReceived;
                progress.ProgressPercentage = progress.BytesReceived.PercentOf(responseLength);
                progress.Speed = progress.BytesReceived / totalTime.Elapsed.TotalSeconds;

                transferProgress.Report(progress);
            }

            return result;
        }

        internal struct Chunk
        {
            public Chunk(int index, int from, int to)
            {
                Index = index;
                From = from; To = to;
                Length = To - From + 1;
            }

            public int From { get; }
            public int To { get; }
            public int Index { get; }

            public int Length { get; }
        }

        public static HttpRequestMessage Clone(HttpRequestMessage req, string newUri = null)
        {
            HttpRequestMessage clone = new HttpRequestMessage(req.Method, newUri ?? req.RequestUri.ToString());

            if (req.Method != HttpMethod.Get)
            {
                clone.Content = req.Content;
            }
            clone.Version = req.Version;

            foreach (KeyValuePair<string, object> prop in req.Properties)
            {
                clone.Properties.Add(prop);
            }

            foreach (KeyValuePair<string, IEnumerable<string>> header in req.Headers)
            {
                clone.Headers.TryAddWithoutValidation(header.Key, header.Value);
            }

            return clone;
        }
    }
}
