using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
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
        private readonly IRestServiceSettings restServiceSettings;

        private const int ChunkSize = 100 * 1000;

        public FastBinaryFilesHttpHandler(IHttpClientFactory clientFactory,
            IRestServiceSettings restServiceSettings,
            IHttpStatistician httpStatistician)
        {
            this.restServiceSettings = restServiceSettings;
         
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
                http.CancelPendingRequests();
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

            response.Dispose();

            var result = new byte[responseLength];

            // blocking collection with back pressure - i.e. adding will blocked while queue is full
            var chunksQueue = new BlockingCollection<Chunk>(this.restServiceSettings.MaxDegreeOfParallelism);
            var downloaders = new List<Task>();
            var downloaderIds = 0;
            var totalTime = new Stopwatch();
            long totalBytesReceived = 0;

            // we will calculate average chunk download speed in bytes/sec
            var avgChunkDownloadSpeed = new SimpleRunningAverage(5);
            avgChunkDownloadSpeed.Add(ChunkSize);

            async Task ChunkDownloader()
            {
                var id = Interlocked.Increment(ref downloaderIds);

                var chunkTimer = new Stopwatch();

                var progress = new TransferProgress { TotalBytesToReceive = responseLength };

                foreach (var chunk in chunksQueue.GetConsumingEnumerable())
                {
                    var rangeRequest = Clone(originalRequest);
                    rangeRequest.Headers.Range = new RangeHeaderValue(chunk.From, chunk.To);

                    chunkTimer.Restart();

                    var block = await http.SendAsync(rangeRequest, HttpCompletionOption.ResponseHeadersRead, token);

                    //var ms = new MemoryStream(); // memory stream do not handle any unmanaged resource, no need to dispose
                    var buffer = await block.Content.ReadAsByteArrayAsync(); // fastest way to read whole response without allocating byte[] twice.

                    Array.Copy(buffer, 0, result, chunk.From, chunk.Length);

                    //ms.Seek(0, SeekOrigin.Begin);
                    
                    //int read, chunkPosition = chunk.From;

                    //while ((read = ms.Read(result, chunkPosition, chunk.To - chunkPosition + 1)) > 0)
                    //{
                    //    chunkPosition += read;

                    //    if (chunkPosition == chunk.To + 1) break;
                    //}

                    chunkTimer.Stop();

                    totalBytesReceived = Interlocked.Add(ref totalBytesReceived, chunk.Length);
                    avgChunkDownloadSpeed.Add(chunk.Length / chunkTimer.Elapsed.TotalSeconds);

                    Debug.WriteLine($"Downloader#{id} - Chunk#{chunk.Index}[{chunk.From} - {chunk.To} = {chunk.Length}] - {totalBytesReceived}");
                    token.ThrowIfCancellationRequested();

                    if (transferProgress == null) continue;
                    progress.BytesReceived = totalBytesReceived;
                    progress.ProgressPercentage = progress.BytesReceived.PercentOf(responseLength);
                    progress.Speed = progress.BytesReceived / totalTime.Elapsed.TotalSeconds;

                    transferProgress.Report(progress);
                }                
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

                token.ThrowIfCancellationRequested();

                // setting new chunk size so that 
                chunkSize = Math.Max((int)(avgChunkDownloadSpeed.Average * 2), ChunkSize /* 256kb min */);
            }

            chunksQueue.CompleteAdding();

            Debug.WriteLine($"Competed adding, waiting for all");

            await Task.WhenAll(downloaders);
            Debug.WriteLine($"Download completed");

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
