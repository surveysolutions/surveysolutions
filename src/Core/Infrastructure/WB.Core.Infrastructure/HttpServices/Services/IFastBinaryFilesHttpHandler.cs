using System;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;

namespace WB.Core.GenericSubdomains.Portable.Implementation.Services
{
    public interface IFastBinaryFilesHttpHandler
    {
        Task<byte[]> DownloadBinaryDataAsync(HttpClient http, HttpResponseMessage response,
            IProgress<TransferProgress> transferProgress,
            CancellationToken token);
    }
}
