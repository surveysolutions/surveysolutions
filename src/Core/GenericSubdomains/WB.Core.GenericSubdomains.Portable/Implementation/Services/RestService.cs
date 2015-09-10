using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;
using Flurl;
using Flurl.Http;
using WB.Core.GenericSubdomains.Portable.Properties;
using WB.Core.GenericSubdomains.Portable.Services;

namespace WB.Core.GenericSubdomains.Portable.Implementation.Services
{
    public class RestService : IRestService
    {
        private readonly IRestServiceSettings restServiceSettings;
        private readonly INetworkService networkService;
        private readonly IJsonUtils jsonUtils;
        private readonly IStringCompressor stringCompressor;

        public RestService(
            IRestServiceSettings restServiceSettings, 
            INetworkService networkService,
            IJsonUtils jsonUtils,
            IStringCompressor stringCompressor,
            IRestServicePointManager restServicePointManager)
        {
            this.restServiceSettings = restServiceSettings;
            this.networkService = networkService;
            this.jsonUtils = jsonUtils;
            this.stringCompressor = stringCompressor;

            if (this.restServiceSettings.AcceptUnsignedSslCertificate && restServicePointManager != null)
                restServicePointManager.AcceptUnsignedSslCertificate();
        }

        private async Task<HttpResponseMessage> ExecuteRequestAsync(
            string url, 
            Func<FlurlClient, Task<HttpResponseMessage>> request,
            object queryString = null, 
            RestCredentials credentials = null)
        {
            if (this.networkService != null && !this.networkService.IsNetworkEnabled())
            {
                throw new RestException(Resources.NoNetwork);
            }

            var fullUrl = this.restServiceSettings.Endpoint
                .AppendPathSegment(url)
                .SetQueryParams(queryString);

            var restClient = fullUrl
                .WithTimeout(this.restServiceSettings.Timeout)
                .WithHeader("Accept-Encoding", "gzip,deflate");

            if (credentials != null)
                restClient.WithBasicAuth(credentials.Login, credentials.Password);

            try
            {
                return await request(restClient);
            }
            catch (FlurlHttpTimeoutException ex)
            {
                throw new RestException(message: "Request timeout", statusCode: HttpStatusCode.RequestTimeout, innerException: ex);
            }
            catch (FlurlHttpException ex)
            {
                if (ex.Call.Response != null)
                    throw new RestException(ex.Call.Response.ReasonPhrase, statusCode: ex.Call.Response.StatusCode,
                        innerException: ex);

                throw new RestException(message: "No connection", innerException: ex);
            }
            catch (WebException ex)
            {
                throw new RestException(message: "No connection", innerException: ex);
            }
        }

        public async Task<T> GetAsync<T>(string url, object queryString = null, RestCredentials credentials = null)
        {
            return await this.GetAsync<T>(url: url, queryString: queryString, credentials: credentials, token: default(CancellationToken));
        }

        public async Task GetAsync(string url, object queryString = null, RestCredentials credentials = null)
        {
            await this.GetAsync(url: url, queryString: queryString, credentials: credentials, token: default(CancellationToken));
        }

        public async Task<T> PostAsync<T>(string url, object request = null, RestCredentials credentials = null)
        {
            return await this.PostAsync<T>(url: url, credentials: credentials, request: request, token: default(CancellationToken));
        }

        public async Task PostAsync(string url, object request = null, RestCredentials credentials = null)
        {
            await this.PostAsync(url: url, credentials: credentials, request: request, token: default(CancellationToken));
        }

        public async Task<T> GetAsync<T>(string url, CancellationToken token, object queryString = null, RestCredentials credentials = null)
        {
            var response = this.ExecuteRequestAsync(url: url, queryString: queryString, credentials: credentials, request: (client) => client.GetAsync(token));
            return await this.ReceiveCompressedJsonAsync<T>(response: response);
        }

        public async Task GetAsync(string url, CancellationToken token, object queryString = null, RestCredentials credentials = null)
        {
            await this.ExecuteRequestAsync(url: url, queryString: queryString, credentials: credentials, request: (client) => client.GetAsync(token));
        }

        public async Task<T> PostAsync<T>(string url, CancellationToken token, object request = null, RestCredentials credentials = null)
        {
            var response = this.ExecuteRequestAsync(url: url, credentials: credentials, request: (client) => client.PostJsonAsync(request, token));
            return await this.ReceiveCompressedJsonAsync<T>(response: response);
        }

        public async Task PostAsync(string url, CancellationToken token, object request = null, RestCredentials credentials = null)
        {
            await this.ExecuteRequestAsync(url: url, credentials: credentials, request: (client) => client.PostJsonAsync(request, token));
        }

        public async Task<T> GetWithProgressAsync<T>(string url, CancellationToken token, Action<DownloadProgressChangedEventArgs> onDownloadProgressChanged, object queryString = null,
            RestCredentials credentials = null)
        {
            var response = this.ExecuteRequestAsync(url: url, queryString: queryString, credentials: credentials, request: (client) => client.GetAsync(token));
            return await this.ReceiveCompressedJsonWithProgressAsync<T>(response: response, token: token, onDownloadProgressChanged: onDownloadProgressChanged);
        }

        public async Task<T> PostWithProgressAsync<T>(string url, CancellationToken token, Action<DownloadProgressChangedEventArgs> onDownloadProgressChanged, object request = null,
            RestCredentials credentials = null)
        {
            var response = this.ExecuteRequestAsync(url: url, credentials: credentials, request: (client) => client.PostJsonAsync(request, token));
            return await this.ReceiveCompressedJsonWithProgressAsync<T>(response: response, token: token, onDownloadProgressChanged: onDownloadProgressChanged);
        }

        public async Task<byte[]> DownloadFileWithProgressAsync(string url, CancellationToken token,
            Action<DownloadProgressChangedEventArgs> onDownloadProgressChanged, RestCredentials credentials = null)
        {
            var response = this.ExecuteRequestAsync(url: url, credentials: credentials, request: (client) => client.SendAsync(HttpMethod.Get, null, token));
            return await this.ReceiveBytesWithProgressAsync(response: response, token: token, onDownloadProgressChanged: onDownloadProgressChanged);
        }

        private async Task<T> ReceiveCompressedJsonAsync<T>(Task<HttpResponseMessage> response)
        {
            var responseMessage = await response;
            var responseContent = await responseMessage.Content.ReadAsByteArrayAsync();

            return this.GetDecompressedJsonFromHttpResponseMessage<T>(responseMessage, responseContent);
        }


        private async Task<T> ReceiveCompressedJsonWithProgressAsync<T>(Task<HttpResponseMessage> response,
            CancellationToken token, Action<DownloadProgressChangedEventArgs> onDownloadProgressChanged = null)
        {
            var responseMessage = await response;
            var responseContent = await this.GetByteArrayResponseContentWithProgressAsync(token, onDownloadProgressChanged, responseMessage);

            return this.GetDecompressedJsonFromHttpResponseMessage<T>(responseMessage, responseContent);
        }

        private async Task<byte[]> ReceiveBytesWithProgressAsync(Task<HttpResponseMessage> response,
            CancellationToken token, Action<DownloadProgressChangedEventArgs> onDownloadProgressChanged = null)
        {
            var responseMessage = await response;

            return await this.GetByteArrayResponseContentWithProgressAsync(token, onDownloadProgressChanged, responseMessage);
        }

        private async Task<byte[]> GetByteArrayResponseContentWithProgressAsync(CancellationToken token, Action<DownloadProgressChangedEventArgs> onDownloadProgressChanged,
            HttpResponseMessage responseMessage)
        {
            if (token.IsCancellationRequested)
            {
                token.ThrowIfCancellationRequested();
            }

            byte[] responseContent;
            var contentLength = responseMessage.Content.Headers.ContentLength;

            using (var responseStream = await responseMessage.Content.ReadAsStreamAsync())
            {
                if (token.IsCancellationRequested)
                {
                    token.ThrowIfCancellationRequested();
                }

                var buffer = new byte[this.restServiceSettings.BufferSize];
                var downloadProgressChangedEventArgs = new DownloadProgressChangedEventArgs()
                {
                    TotalBytesToReceive = contentLength
                };
                using (var ms = new MemoryStream())
                {
                    int read;
                    while ((read = await responseStream.ReadAsync(buffer, 0, buffer.Length, token)) > 0)
                    {
                        if (token.IsCancellationRequested)
                        {
                            token.ThrowIfCancellationRequested();
                        }

                        ms.Write(buffer, 0, read);

                        if (onDownloadProgressChanged == null) continue;

                        downloadProgressChangedEventArgs.BytesReceived = ms.Length;
                        downloadProgressChangedEventArgs.ProgressPercentage = Math.Round((decimal)(100 * ms.Length) / contentLength.Value);
                        onDownloadProgressChanged(downloadProgressChangedEventArgs);
                    }
                    responseContent = ms.ToArray();
                }
            }
            return responseContent;
        }

        private T GetDecompressedJsonFromHttpResponseMessage<T>(HttpResponseMessage responseMessage, byte[] responseContent)
        {
            var responseContentType = responseMessage.Content.Headers.ContentType.MediaType;

            if (responseContentType.IndexOf("json", StringComparison.OrdinalIgnoreCase) > -1 ||
                responseContentType.IndexOf("javascript", StringComparison.OrdinalIgnoreCase) > -1)
            {
                IEnumerable<string> acceptedEncodings;
                if (responseMessage.Content.Headers.TryGetValues("Content-Encoding", out acceptedEncodings))
                {
                    if (acceptedEncodings.Contains("gzip"))
                    {
                        responseContent = this.stringCompressor.DecompressGZip(responseContent);
                    }

                    if (acceptedEncodings.Contains("deflate"))
                    {
                        responseContent = this.stringCompressor.DecompressDeflate(responseContent);
                    }
                }

                try
                {
                    return this.jsonUtils.Deserialize<T>(responseContent);
                }
                catch (JsonDeserializationException ex)
                {
                    throw new RestException(message: Resources.UpdateRequired,
                        statusCode: HttpStatusCode.UpgradeRequired, innerException: ex);
                }
            }


            throw new RestException(message: Resources.CheckServerSettings, statusCode: HttpStatusCode.Redirect);
        }
    }
}
