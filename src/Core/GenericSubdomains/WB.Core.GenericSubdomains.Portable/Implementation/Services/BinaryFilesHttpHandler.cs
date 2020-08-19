using System;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;

namespace WB.Core.GenericSubdomains.Portable.Implementation.Services
{
    public class BinaryFilesHttpHandler : IFastBinaryFilesHttpHandler
    {
        public Task<byte[]> DownloadBinaryDataAsync(HttpClient http, HttpResponseMessage response, IProgress<TransferProgress> transferProgress,
            CancellationToken token)
        {
            return response.Content.ReadAsByteArrayAsync();
        }
    }
}
