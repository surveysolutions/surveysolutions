using System;
using System.Collections.Generic;
using System.IO;
using System.Net;
using System.Threading;
using System.Threading.Tasks;
using Moq;
using WB.Core.GenericSubdomains.Portable.Implementation;
using WB.Core.GenericSubdomains.Portable.Services;

namespace WB.Tests.Unit.Applications.Headquarters.ApiTests
{
    public class DummyRestSeviceForCollectUrls : IRestService
    {
        private readonly HashSet<string> urls = new HashSet<string>();

        public IEnumerable<string> Urls => urls;

        public bool IsValidHostAddress(string url)
        {
            throw new NotImplementedException();
        }

        public Task GetAsync(string url, object queryString = null, RestCredentials credentials = null, bool forceNoCache = false,
            Dictionary<string, string> customHeaders = null, CancellationToken? token = null)
        {
            AddUrl(url);
            return Task.CompletedTask;
        }

        public Task PostAsync(string url, object request = null, RestCredentials credentials = null, CancellationToken? token = null)
        {
            AddUrl(url);
            return Task.CompletedTask;
        }

        public Task<T> GetAsync<T>(string url, IProgress<TransferProgress> transferProgress = null, object queryString = null,
            RestCredentials credentials = null, CancellationToken? token = null)
        {
            AddUrl(url);
            return Task.FromResult(default(T));
        }

        public Task<T> PostAsync<T>(string url, IProgress<TransferProgress> transferProgress = null, object request = null,
            RestCredentials credentials = null, CancellationToken? token = null)
        {
            AddUrl(url);
            return Task.FromResult(default(T));
        }

        public Task<RestFile> DownloadFileAsync(string url, IProgress<TransferProgress> transferProgress = null, RestCredentials credentials = null,
            CancellationToken? token = null, Dictionary<string, string> customHeaders = null)
        {
            AddUrl(url);
            return Task.FromResult(new RestFile(new byte[0], "content-type", "hash", 2, "name", HttpStatusCode.Found));
        }

        public Task<RestStreamResult> GetResponseStreamAsync(string url, RestCredentials credentials = null, CancellationToken? token = null,
            object queryString = null, Dictionary<string, string> customHeaders = null)
        {
            AddUrl(url);
            return Task.FromResult(new RestStreamResult());
        }

        public Task SendStreamAsync(Stream stream, string url, RestCredentials credentials, Dictionary<string, string> customHeaders = null,
            CancellationToken? token = null)
        {
            AddUrl(url);
            return Task.CompletedTask;
        }

        private void AddUrl(string url)
        {
            urls.Add(url);
        }
    }

    public class DummyTransferProgress : IProgress<TransferProgress>
    {
        public void Report(TransferProgress value)
        {

        }
    }
}
