using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using WB.Core.GenericSubdomains.Portable.Implementation.Compression;
using WB.Core.GenericSubdomains.Portable.Properties;
using WB.Core.GenericSubdomains.Portable.Services;

namespace WB.Core.GenericSubdomains.Portable.Implementation.Services
{
    public class RestService : IRestService
    {
        private readonly IRestServiceSettings restServiceSettings;
        private readonly INetworkService networkService;
        private readonly IJsonAllTypesSerializer synchronizationSerializer;
        private readonly IStringCompressor stringCompressor;
        private readonly IHttpStatistician httpStatistician;
        private readonly ILogger logger;

        public RestService(
            IRestServiceSettings restServiceSettings,
            INetworkService networkService,
            IJsonAllTypesSerializer synchronizationSerializer,
            IStringCompressor stringCompressor,
            IRestServicePointManager restServicePointManager,
            IHttpStatistician httpStatistician,
            ILogger logger)
        {
            this.restServiceSettings = restServiceSettings;
            this.networkService = networkService;
            this.synchronizationSerializer = synchronizationSerializer;
            this.stringCompressor = stringCompressor;
            this.httpStatistician = httpStatistician;
            this.logger = logger;

            if (this.restServiceSettings.AcceptUnsignedSslCertificate)
                restServicePointManager?.AcceptUnsignedSslCertificate();
        }

        private Task<HttpResponseMessage> ExecuteRequestAsync(
            string url,
            HttpMethod method,
            object queryString = null,
            object request = null,
            RestCredentials credentials = null,
            bool forceNoCache = false,
            Dictionary<string, string> customHeaders = null,
            CancellationToken? userCancellationToken = null,
            IProgress<TransferProgress> progress = null)
        {
            var compressedJsonContent = this.CreateCompressedJsonContent(request);
            return this.ExecuteRequestAsync(url, method, queryString, compressedJsonContent, credentials, forceNoCache,
                customHeaders, userCancellationToken, progress);
        }

        private async Task<HttpResponseMessage> ExecuteRequestAsync(
            string url,
            HttpMethod method,
            object queryString = null,
            HttpContent httpContent = null,
            RestCredentials credentials = null,
            bool forceNoCache = false,
            Dictionary<string, string> customHeaders = null,
            CancellationToken? userCancellationToken = null,
            IProgress<TransferProgress> progress = null)
        {
            if (!this.IsValidHostAddress(this.restServiceSettings.Endpoint))
                throw new RestException("Invalid URL", type: RestExceptionType.InvalidUrl);

            if (this.networkService != null)
            {
                if (!this.networkService.IsNetworkEnabled())
                    throw new RestException("No network", type: RestExceptionType.NoNetwork);

                if (!this.networkService.IsHostReachable(this.restServiceSettings.Endpoint))
                    throw new RestException("Host unreachable", type: RestExceptionType.HostUnreachable);
            }

            var requestTimeoutToken = new CancellationTokenSource(this.restServiceSettings.Timeout).Token;
            var linkedCancellationTokenSource = CancellationTokenSource.CreateLinkedTokenSource(requestTimeoutToken,
                userCancellationToken ?? default(CancellationToken));

            var fullUrl = new Url(this.restServiceSettings.Endpoint, url, queryString);

            var httpClient = new HttpClient(new ExtendedMessageHandler(new HttpClientHandler(), httpStatistician));

            httpClient.Timeout = this.restServiceSettings.Timeout;
            httpClient.DefaultRequestHeaders.ConnectionClose = false;

            var request = new HttpRequestMessage()
            {
                RequestUri = new Uri(fullUrl.ToString()),
                Method = method,
                Content = httpContent
            };

            request.Headers.UserAgent.ParseAdd(this.restServiceSettings.UserAgent);
            
            request.Headers.AcceptEncoding.Add(new StringWithQualityHeaderValue("gzip"));
            request.Headers.AcceptEncoding.Add(new StringWithQualityHeaderValue("deflate"));
            request.Headers.Add("Accept-Language", "en-GB,en;q=0.9,en-US;q=0.8");

            if (forceNoCache)
            {
                request.Headers.CacheControl = new CacheControlHeaderValue() { NoCache = true };
            }

            if (credentials?.Token != null)
            {
                string base64String = Convert.ToBase64String(Encoding.UTF8.GetBytes($"{credentials.Login}:{credentials.Token}"));
                request.Headers.Authorization = new AuthenticationHeaderValue(ApiAuthenticationScheme.AuthToken.ToString(), base64String);
            }
            else if (credentials?.Password != null)
            {
                var value = Convert.ToBase64String(Encoding.UTF8.GetBytes($"{credentials.Login}:{credentials.Password}"));
                request.Headers.Authorization = new AuthenticationHeaderValue("Basic", value);
            }

            if (customHeaders != null)
            {
                foreach (var customHeader in customHeaders)
                {
                    request.Headers.Add(customHeader.Key, customHeader.Value);
                }
            }

            Stopwatch stopwatch = new Stopwatch();

            try
            {
                stopwatch.Start();

                var httpResponseMessage = await httpClient.SendAsync(request, HttpCompletionOption.ResponseHeadersRead, linkedCancellationTokenSource.Token);

                if (httpResponseMessage.IsSuccessStatusCode
                    || httpResponseMessage.StatusCode == HttpStatusCode.NotModified
                    || httpResponseMessage.StatusCode == HttpStatusCode.NoContent)
                {
                    return httpResponseMessage;
                }

                throw new RestException(httpResponseMessage.ReasonPhrase, statusCode: httpResponseMessage.StatusCode);
            }
            catch (OperationCanceledException ex)
            {
                // throwed when receiving bytes in ReceiveBytesWithProgressAsync method and user canceling request
                throw new RestException("Request canceled by user", type: RestExceptionType.RequestCanceledByUser, innerException: ex);
            }
            catch (ExtendedMessageHandlerException ex)
            {
                stopwatch.Stop();
                logger.Error($"ERROR!!! Request to {url} failed. Time: {stopwatch.Elapsed}.", ex);
                logger.Error($"ERROR!!! Request headers {ex?.Call?.Request?.Headers}. Response headers {ex?.Call?.Response?.Headers}.");

                if (ex.GetSelfOrInnerAs<TaskCanceledException>() != null || ex.GetSelfOrInnerAs<OperationCanceledException>() != null)
                {
                    if (requestTimeoutToken.IsCancellationRequested)
                    {
                        throw new RestException("Request timeout", type: RestExceptionType.RequestByTimeout,
                            statusCode: HttpStatusCode.RequestTimeout, innerException: ex);
                    }

                    if (userCancellationToken.HasValue && userCancellationToken.Value.IsCancellationRequested)
                    {
                        throw new RestException("Request canceled by user",
                               type: RestExceptionType.RequestCanceledByUser, innerException: ex);
                    }
                }
                else if (ex.Call.Response != null)
                {
                    throw new RestException(ex.Call.Response.ReasonPhrase, statusCode: ex.Call.Response.StatusCode, innerException: ex);
                }
                else
                {
                    var innerException = ex.InnerException;
                    if (innerException?.GetType().FullName == "Java.Net.ConnectException")
                    {
                        throw new RestException(message: innerException.Message, 
                            innerException: innerException,
                            type: RestExceptionType.HostUnreachable);
                    }

                    if (innerException?.GetType().Name == "SSLHandshakeException")
                    {
                        throw new RestException(message: innerException.Message, 
                            innerException: innerException,
                            type: RestExceptionType.UnacceptableCertificate);
                    }
                }

                throw new RestException(message: "Unexpected web exception", innerException: ex);
            }
        }

        public Task GetAsync(string url, object queryString, RestCredentials credentials, bool forceNoCache, 
            Dictionary<string, string> customHeaders, CancellationToken? token)
        {
            return this.ExecuteRequestAsync(url: url,
                queryString: queryString,
                credentials: credentials,
                method: HttpMethod.Get,
                customHeaders: customHeaders,
                forceNoCache: forceNoCache,
                userCancellationToken: token,
                request: null);
        }

        public Task PostAsync(string url, object request = null, RestCredentials credentials = null, CancellationToken? token = null)
        {
            return this.ExecuteRequestAsync(url: url, 
                credentials: credentials,
                method: HttpMethod.Post,
                request: request,
                userCancellationToken: token);
        }

        public Task<T> GetAsync<T>(string url,
            IProgress<TransferProgress> transferProgress, object queryString = null,
            RestCredentials credentials = null, CancellationToken? token = null)
        {
            var response = this.ExecuteRequestAsync(url: url, queryString: queryString, credentials: credentials, method: HttpMethod.Get,
                userCancellationToken: token, request: null);

            return this.ReceiveCompressedJsonWithProgressAsync<T>(response: response, token: token ?? default(CancellationToken),
                transferProgress: transferProgress);
        }

        public Task<T> PostAsync<T>(string url,
            IProgress<TransferProgress> transferProgress, object request = null,
            RestCredentials credentials = null, CancellationToken? token = null)
        {
            var response = this.ExecuteRequestAsync(url: url, credentials: credentials, method: HttpMethod.Post,
                request: request,progress: transferProgress, userCancellationToken: token);

            return this.ReceiveCompressedJsonWithProgressAsync<T>(response: response, token: token ?? default(CancellationToken),
                transferProgress: transferProgress);
        }

        public async Task<RestFile> DownloadFileAsync(string url,
            IProgress<TransferProgress> transferProgress, 
            RestCredentials credentials = null,
            CancellationToken? token = null,
            Dictionary<string, string> customHeaders = null)
        {
            var response = this.ExecuteRequestAsync(url: url, credentials: credentials, method: HttpMethod.Get,
                userCancellationToken: token, request: null, customHeaders: customHeaders);

            var restResponse = await this.ReceiveBytesWithProgressAsync(response: response, token: token ?? default(CancellationToken),
                        transferProgress: transferProgress).ConfigureAwait(false);

            var fileContent = this.GetDecompressedContentFromHttpResponseMessage(restResponse);

            return new RestFile(content: fileContent, contentType: restResponse.RawContentType,
                contentHash: restResponse.ETag, contentLength: restResponse.Length, fileName: restResponse.FileName, 
                statusCode: restResponse.StatusCode);
        }

        public async Task<RestStreamResult> GetResponseStreamAsync(string url, RestCredentials credentials = null, 
            CancellationToken? ctoken = null, object queryString = null, Dictionary<string, string> customHeaders = null)
        {
            var response = this.ExecuteRequestAsync(url: url, credentials: credentials, method: HttpMethod.Get,
                userCancellationToken: ctoken, request: null, queryString: queryString, customHeaders: customHeaders);
            
            var responseMessage = await response.ConfigureAwait(false);
            var contentLength = responseMessage.Content.Headers.ContentLength;

            var contentCompressionType = this.GetContentCompressionType(responseMessage.Content.Headers);

            var responseStream = await responseMessage.Content.ReadAsStreamAsync().ConfigureAwait(false);

            switch (contentCompressionType)
            {
                case RestContentCompressionType.GZip:
                    return new RestStreamResult
                    {
                        Stream = this.stringCompressor.GetDecompressingGZipStream(responseStream),
                        ContentLength = contentLength
                    };

                case RestContentCompressionType.Deflate:
                    return new RestStreamResult
                    {
                        Stream = this.stringCompressor.GetDecompressingDeflateStream(responseStream),
                        ContentLength = contentLength
                    };
                default:
                    return new RestStreamResult
                    {
                        Stream = responseStream,
                        ContentLength = contentLength
                    };
            }
        }

        public async Task SendStreamAsync(Stream streamData, string url, RestCredentials credentials,
            Dictionary<string, string> customHeaders = null, CancellationToken? token = null)
        {
            using (var multipartFormDataContent = new MultipartFormDataContent())
            {
                using (HttpContent streamContent = new StreamContent(streamData))
                {
                    streamContent.Headers.Add("Content-Type", "application/octet-stream");
                    streamContent.Headers.Add("Content-Length", streamData.Length.ToString());
                    streamContent.Headers.ContentDisposition = new ContentDispositionHeaderValue("attachment")
                    {
                        FileName = "backup.zip"
                    };
                    multipartFormDataContent.Add(streamContent);

                    await this.ExecuteRequestAsync(url: url, queryString: null, credentials: credentials,
                        method: HttpMethod.Post, forceNoCache: true, userCancellationToken: token,
                        customHeaders: customHeaders, httpContent: multipartFormDataContent).ConfigureAwait(false);
                }
            }
        }

        private HttpContent CreateCompressedJsonContent(object data)
        {
            return data == null
                ? null
                : new CompressedContent(
                    new StringContent(this.synchronizationSerializer.Serialize(data), Encoding.UTF8, "application/json"), new GZipCompressor());
        }

        private async Task<T> ReceiveCompressedJsonWithProgressAsync<T>(Task<HttpResponseMessage> response,
            CancellationToken token, IProgress<TransferProgress> transferProgress = null)
        {
            var restResponse = await this.ReceiveBytesWithProgressAsync(token, transferProgress, response).ConfigureAwait(false);

            return this.GetDecompressedJsonFromHttpResponseMessage<T>(restResponse);
        }

        private async Task<RestResponse> ReceiveBytesWithProgressAsync(CancellationToken token, IProgress<TransferProgress> transferProgress,
            Task<HttpResponseMessage> response)
        {
            var responseMessage = await response.ConfigureAwait(false);
            byte[] responseContent;
            var contentLength = responseMessage.Content.Headers.ContentLength;

            using (var responseStream = await responseMessage.Content.ReadAsStreamAsync().ConfigureAwait(false))
            {
                if (token.IsCancellationRequested)
                {
                    token.ThrowIfCancellationRequested();
                }

                var buffer = new byte[this.restServiceSettings.BufferSize];
                var downloadProgressChangedEventArgs = new TransferProgress
                {
                    TotalBytesToReceive = contentLength
                };
                using (var ms = new MemoryStream())
                {
                    int read;
                    while ((read = await responseStream.ReadAsync(buffer, 0, buffer.Length, token).ConfigureAwait(false)) > 0)
                    {
                        if (token.IsCancellationRequested)
                        {
                            token.ThrowIfCancellationRequested();
                        }

                        ms.Write(buffer, 0, read);

                        if (transferProgress == null) continue;
                        
                        if (contentLength != null)
                            downloadProgressChangedEventArgs.ProgressPercentage = Math.Round((decimal) (100 * ms.Length) / contentLength.Value, 2);

                        downloadProgressChangedEventArgs.BytesReceived = ms.Length;
                        transferProgress.Report(downloadProgressChangedEventArgs);
                    }
                    responseContent = ms.ToArray();
                }
            }

            return new RestResponse
            {
                Response = responseContent,
                ContentType = this.GetContentType(responseMessage.Content.Headers.ContentType?.MediaType),
                ContentCompressionType = this.GetContentCompressionType(responseMessage.Content.Headers),
                RawContentType = responseMessage.Content?.Headers?.ContentType?.MediaType,
                Length = responseMessage.Content?.Headers?.ContentLength,
                ETag = responseMessage.Headers?.ETag?.Tag,
                FileName = responseMessage.Content?.Headers?.ContentDisposition?.FileName,
                StatusCode = responseMessage.StatusCode
            };
        }

        private RestContentType GetContentType(string mediaType)
        {
            if (mediaType.IsNullOrEmpty())
                return RestContentType.Unknown;

            if (mediaType.IndexOf("json", StringComparison.OrdinalIgnoreCase) > -1 || mediaType.IndexOf("javascript", StringComparison.OrdinalIgnoreCase) > -1)
                return RestContentType.Json;

            return RestContentType.Unknown;
        }

        private RestContentCompressionType GetContentCompressionType(HttpContentHeaders headers)
        {
            headers.TryGetValues("Content-Encoding", out var acceptedEncodings);

            if (acceptedEncodings == null) return RestContentCompressionType.None;

            if (acceptedEncodings.Contains("gzip"))
                return RestContentCompressionType.GZip;

            if (acceptedEncodings.Contains("deflate"))
                return RestContentCompressionType.Deflate;

            return RestContentCompressionType.None;
        }

        private T GetDecompressedJsonFromHttpResponseMessage<T>(RestResponse restResponse)
        {
            if (restResponse.ContentType != RestContentType.Json)
                throw new RestException(message: Resources.CheckServerSettings, statusCode: HttpStatusCode.Redirect);

            try
            {
                var responseContent = GetDecompressedContentFromHttpResponseMessage(restResponse);
                return this.synchronizationSerializer.Deserialize<T>(responseContent);
            }
            catch (JsonDeserializationException ex)
            {
                throw new RestException(message: Resources.UpdateRequired, statusCode: HttpStatusCode.UpgradeRequired, innerException: ex);
            }
        }


        private byte[] GetDecompressedContentFromHttpResponseMessage(RestResponse restResponse)
        {
            var responseContent = restResponse.Response;

            switch (restResponse.ContentCompressionType)
            {
                case RestContentCompressionType.GZip:
                    responseContent = this.stringCompressor.DecompressGZip(responseContent);
                    break;
                case RestContentCompressionType.Deflate:
                    responseContent = this.stringCompressor.DecompressDeflate(responseContent);
                    break;
            }

            return responseContent;
        }

        public bool IsValidHostAddress(string url)
            => Uri.TryCreate(url, UriKind.Absolute, out var uriResult) &&
               (uriResult.Scheme == "http" || uriResult.Scheme == "https");

        internal class RestResponse
        {
            public byte[] Response { get; set; }
            public RestContentType ContentType { get; set; }
            public RestContentCompressionType ContentCompressionType { get; set; }
            public string RawContentType { get; set; }
            public long? Length { get; set; }
            public string ETag { get; set; }
            public string FileName { get; set; }
            public HttpStatusCode StatusCode { get; set; }
        }

        internal enum RestContentType { Unknown, Json }
        internal enum RestContentCompressionType { None, GZip, Deflate }
    }
}
