using System;
using System.Buffers;
using System.Diagnostics;
using System.IO;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;
using WB.Core.GenericSubdomains.Portable;
using WB.Core.GenericSubdomains.Portable.Implementation;

namespace WB.Core.Infrastructure.HttpServices.Services
{
    public class BinaryFilesHttpHandler : IFastBinaryFilesHttpHandler
    {
        public async Task<byte[]> DownloadBinaryDataAsync(System.Net.Http.HttpClient http, HttpResponseMessage response,
            IProgress<TransferProgress> transferProgress,
            CancellationToken token)
        {
            await using Stream contentStream = await response.Content.ReadAsStreamAsync();
            await using MemoryStream memoryStream = new MemoryStream();

            var totalRead = 0L;
            var batchCount = 0L;
            var buffer = new byte[16 * 1024];
            var isMoreToRead = true;

            var totalBytesToReceive = response.Content.Headers.ContentLength;
            var stopWatch = Stopwatch.StartNew();
            do
            {
                var read = await contentStream.ReadAsync(buffer, 0, buffer.Length, token);
                if (read == 0)
                {
                    isMoreToRead = false;
                }
                else
                {
                    await memoryStream.WriteAsync(buffer, 0, read, token);

                    totalRead += read;
                    batchCount++;
                    
                    decimal progress = 0;
                    if (totalBytesToReceive.HasValue)
                    {
                        progress = totalRead.PercentOf(totalBytesToReceive.Value);
                    }

                    transferProgress?.Report(new TransferProgress
                    { 
                        BytesReceived = totalRead,
                        ProgressPercentage = progress, 
                        TotalBytesToReceive = totalBytesToReceive,
                        Speed = buffer.Length * batchCount / stopWatch.Elapsed.TotalSeconds
                    });
                }
            } while (isMoreToRead);
            
            return memoryStream.ToArray();
        }
    }
}
