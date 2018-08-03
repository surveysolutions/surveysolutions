using System;
using System.Collections.Generic;
using System.IO;
using System.Threading;
using System.Threading.Tasks;
using WB.Core.GenericSubdomains.Portable.Implementation;

namespace WB.Core.GenericSubdomains.Portable.Services
{
    public interface IRestService
    {
        bool IsValidHostAddress(string url);
        Task GetAsync(string url, object queryString = null, 
            RestCredentials credentials = null, bool forceNoCache = false, 
            Dictionary<string, string> customHeaders = null, CancellationToken ? token = null);

        Task PostAsync(string url, object request = null, RestCredentials credentials = null, CancellationToken? token = null);

        Task<T> GetAsync<T>(string url, IProgress<TransferProgress> transferProgress = null,
            object queryString = null, RestCredentials credentials = null, CancellationToken? token = null);

        Task<T> PostAsync<T>(string url, IProgress<TransferProgress> transferProgress = null,
            object request = null, RestCredentials credentials = null, CancellationToken? token = null);

        Task<RestFile> DownloadFileAsync(string url,
            IProgress<TransferProgress> transferProgress = null,
            RestCredentials credentials = null, 
            CancellationToken? token = null, 
            Dictionary<string, string> customHeaders = null);

        Task<RestStreamResult> GetResponseStreamAsync(string url,
            RestCredentials credentials = null,
            CancellationToken? token = null,
            object queryString = null,
            Dictionary<string, string> customHeaders = null);

        Task SendStreamAsync(Stream stream, string url, 
            RestCredentials credentials, 
            Dictionary<string, string> customHeaders = null, 
            CancellationToken? token = null);
    }
}
