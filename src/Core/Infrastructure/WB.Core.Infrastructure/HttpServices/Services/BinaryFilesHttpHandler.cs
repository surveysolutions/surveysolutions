using System;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;
using WB.Core.GenericSubdomains.Portable.Implementation;

namespace WB.Core.Infrastructure.HttpServices.Services
{
    public class BinaryFilesHttpHandler : IFastBinaryFilesHttpHandler
    {
        public Task<byte[]> DownloadBinaryDataAsync(System.Net.Http.HttpClient http, HttpResponseMessage response, IProgress<TransferProgress> transferProgress,
            CancellationToken token)
        {
            return response.Content.ReadAsByteArrayAsync();
        }
    }
}
