using System;
using System.Threading;
using System.Threading.Tasks;

using WB.Core.GenericSubdomains.Portable.Implementation;

namespace WB.Core.GenericSubdomains.Portable.Services
{
    public interface IRestService
    {
        Task GetAsync(string url, object queryString = null, RestCredentials credentials = null, bool forceNoCache = false, CancellationToken? token = null);
        Task PostAsync(string url, object request = null, RestCredentials credentials = null, CancellationToken? token = null);

        Task<T> GetAsync<T>(string url, Action<DownloadProgressChangedEventArgs> onDownloadProgressChanged = null,
            object queryString = null, RestCredentials credentials = null, CancellationToken? token = null);

        Task<T> PostAsync<T>(string url, Action<DownloadProgressChangedEventArgs> onDownloadProgressChanged = null,
            object request = null, RestCredentials credentials = null, CancellationToken? token = null);

        Task<byte[]> DownloadFileAsync(string url,
            Action<DownloadProgressChangedEventArgs> onDownloadProgressChanged = null,
            RestCredentials credentials = null, CancellationToken? token = null);
    }
}