using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Security.Cryptography;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using WB.Core.GenericSubdomains.Portable;
using WB.Core.GenericSubdomains.Portable.Implementation;
using WB.Core.GenericSubdomains.Portable.Implementation.Compression;
using WB.Core.GenericSubdomains.Portable.Services;
using WB.Core.Infrastructure.HttpServices.HttpClient;

namespace WB.Core.Infrastructure.HttpServices.Services
{
    public class RestService : IRestService
    {
        private class ResponseWithErrorMessage
        {
            public string Message { get; set; }
        }
        private readonly IRestServiceSettings restServiceSettings;
        private readonly INetworkService networkService;
        private readonly IJsonAllTypesSerializer synchronizationSerializer;
        private readonly IStringCompressor stringCompressor;
        private readonly IHttpStatistician httpStatistician;
        private readonly IHttpClientFactory httpClientFactory;
        private readonly IFastBinaryFilesHttpHandler fileDownloader;
        private ILogger logger;

        public RestService(
            IRestServiceSettings restServiceSettings,
            INetworkService networkService,
            IJsonAllTypesSerializer synchronizationSerializer,
            IStringCompressor stringCompressor,
            IRestServicePointManager restServicePointManager,
            IHttpStatistician httpStatistician,
            IHttpClientFactory httpClientFactory, 
            IFastBinaryFilesHttpHandler fileDownloader,
            ILogger logger)
        {
            this.restServiceSettings = restServiceSettings;
            this.networkService = networkService;
            this.synchronizationSerializer = synchronizationSerializer;
            this.stringCompressor = stringCompressor;
            this.httpStatistician = httpStatistician;
            this.httpClientFactory = httpClientFactory;
            this.fileDownloader = fileDownloader;
            this.logger = logger;

            if (this.restServiceSettings.AcceptUnsignedSslCertificate)
                restServicePointManager?.AcceptUnsignedSslCertificate();
        }

        private Task<ExecuteRequestResult> ExecuteRequestAsync(
            string url,
            HttpMethod method,
            object queryString = null,
            object request = null,
            RestCredentials credentials = null,
            bool forceNoCache = false,
            Dictionary<string, string> customHeaders = null,
            CancellationToken? userCancellationToken = null)
        {
            var compressedJsonContent = this.CreateCompressedJsonContent(request);
            return this.ExecuteRequestAsync(url, method, queryString, compressedJsonContent, credentials, forceNoCache,
                customHeaders, userCancellationToken);
        }

        private async Task<ExecuteRequestResult> ExecuteRequestAsync(
            string url,
            HttpMethod method,
            object queryString = null,
            HttpContent httpContent = null,
            RestCredentials credentials = null,
            bool forceNoCache = false,
            Dictionary<string, string> customHeaders = null,
            CancellationToken? userCancellationToken = null)
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
                userCancellationToken ?? default);

            var fullUrl = new Url(this.restServiceSettings.Endpoint, (credentials?.Workspace ?? "") + "/" + url, queryString);

            var request = new HttpRequestMessage()
            {
                RequestUri = new Uri(fullUrl.ToString()),
                Method = method,
                Content = httpContent
            };

            request.Headers.UserAgent.ParseAdd(this.restServiceSettings.UserAgent);
            request.Headers.AcceptEncoding.Add(new StringWithQualityHeaderValue("gzip"));
            request.Headers.AcceptEncoding.Add(new StringWithQualityHeaderValue("deflate"));

            try
            {
                if (customHeaders != null && customHeaders.TryGetValue("If-None-Match", out var etag))
                {
                    string tag;
                    if (etag.Length > 0 && etag[0] == '"' && etag[etag.Length - 1] == '"')
                    {
                        tag = etag;
                    }
                    else
                    {
                        tag = $@"""{etag}""";
                    }

                    request.Headers.IfNoneMatch.Add(new EntityTagHeaderValue(tag));
                    customHeaders.Remove("If-None-Match");
                }

                if (forceNoCache)
                {
                    request.Headers.CacheControl = new CacheControlHeaderValue { NoCache = true };
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
                var httpClient = this.httpClientFactory.CreateClient(httpStatistician);

                var httpResponseMessage = await
                    httpClient.SendAsync(request, HttpCompletionOption.ResponseHeadersRead, linkedCancellationTokenSource.Token)
                        .ConfigureAwait(false);
                this.logger.Debug($"Executed web request url: {request.RequestUri}, response code: {httpResponseMessage.StatusCode}");

                if (httpResponseMessage.IsSuccessStatusCode
                    || httpResponseMessage.StatusCode == HttpStatusCode.NotModified
                    || httpResponseMessage.StatusCode == HttpStatusCode.NoContent)
                {
                    return new ExecuteRequestResult(httpClient, httpResponseMessage);
                }

                throw new RestException(httpResponseMessage.ReasonPhrase, statusCode: httpResponseMessage.StatusCode);
            }
            catch (OperationCanceledException ex)
            {
                this.logger.Debug("OperationCanceledException", ex);
                // throwed when receiving bytes in ReceiveBytesWithProgressAsync method and user canceling request
                throw new RestException("Request canceled by user", type: RestExceptionType.RequestCanceledByUser, innerException: ex);
            }
            catch (ExtendedMessageHandlerException ex)
            {
                this.logger.Debug("ExtendedMessageHandlerException", ex);
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
                    var reasonPhrase = GetReasonPhrase(ex);
                    var restException = new RestException(reasonPhrase, statusCode: ex.Call.Response.StatusCode, innerException: ex);

                    if (ex.Call.Response.Headers.Contains("version"))
                        restException.Data["target-version"] = ex.Call.Response.Headers.GetValues("version").FirstOrDefault();
                    
                    throw restException;
                }
                else
                {
                    var javaNetConnectionException = ex.GetSelfOrInner(e => e?.GetType().FullName == "Java.Net.ConnectException");
                    if (javaNetConnectionException != null)
                    {
                        throw new RestException(message: javaNetConnectionException.Message,
                            innerException: javaNetConnectionException,
                            type: RestExceptionType.HostUnreachable);
                    }

                    var sslHandshakeException = ex.GetSelfOrInner(e => e?.GetType().Name == "SSLHandshakeException");
                    if (sslHandshakeException != null)
                    {
                        throw new RestException(message: sslHandshakeException.Message,
                            innerException: sslHandshakeException,
                            type: RestExceptionType.UnacceptableCertificate);
                    }
                }

                throw new RestException(message: "Unexpected web exception", innerException: ex);
            }
        }

        private T GetReasonObject<T>(ExtendedMessageHandlerException ex) where T: class
        {
            try
            {
                var responseMessage = ex.Call.Response;
                var responseContent = responseMessage.Content.ReadAsByteArrayAsync().Result;
                var restContentCompressionType = this.GetContentCompressionType(responseMessage.Content.Headers);
                var decompressedContent = DecompressedContentFromHttpResponseMessage(restContentCompressionType, responseContent);
                return this.synchronizationSerializer.Deserialize<T>(decompressedContent);
            }
            catch
            {
                return null;
            } 
        }
        
        private string GetReasonPhrase(ExtendedMessageHandlerException ex)
        {
            var reasonObject = GetReasonObject<ResponseWithErrorMessage>(ex);
            if (reasonObject != null)
                return reasonObject.Message;

            return ex.Call.Response.ReasonPhrase;
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

        public Task PostAsync(string url, object request, RestCredentials credentials = null, CancellationToken? token = null)
        {
            return this.ExecuteRequestAsync(url: url,
                credentials: credentials,
                method: HttpMethod.Post,
                request: request ?? string.Empty,
                userCancellationToken: token);
        }

        public async Task<T> GetAsync<T>(string url,
            IProgress<TransferProgress> transferProgress, object queryString = null,
            RestCredentials credentials = null, Dictionary<string, string> customHeaders = null, CancellationToken ? token = null)
        {
            var response = await this.ExecuteRequestAsync(url: url, queryString: queryString, credentials: credentials, method: HttpMethod.Get,
                userCancellationToken: token, request: null, customHeaders: customHeaders).ConfigureAwait(false); ;

            return await this.ReceiveCompressedJsonWithProgressAsync<T>(response: response, token: token ?? default,
                transferProgress: transferProgress).ConfigureAwait(false); ;
        }

        public async Task<T> PostAsync<T>(string url,
            IProgress<TransferProgress> transferProgress, object request,
            RestCredentials credentials = null, CancellationToken? token = null)
        {
            var response = await this.ExecuteRequestAsync(url: url, credentials: credentials, method: HttpMethod.Post,
                request: request ?? string.Empty, userCancellationToken: token)
                .ConfigureAwait(false);

            return await this.ReceiveCompressedJsonWithProgressAsync<T>(response: response, token: token ?? default,
                transferProgress: transferProgress).ConfigureAwait(false);
        }

        public async Task<RestFile> DownloadFileAsync(string url,
            IProgress<TransferProgress> transferProgress,
            RestCredentials credentials = null,
            CancellationToken? token = null,
            Dictionary<string, string> customHeaders = null)
        {
            var response = await this.ExecuteRequestAsync(url: url, credentials: credentials, method: HttpMethod.Get,
                userCancellationToken: token, request: null, customHeaders: customHeaders).ConfigureAwait(false);

            var restResponse = await this.ReceiveBytesWithProgressAsync(response,
                        transferProgress: transferProgress, token: token ?? default)
                .ConfigureAwait(false);

            if (restResponse.StatusCode == HttpStatusCode.NotModified)
            {
                return new RestFile(null, string.Empty, null, null, null, restResponse.StatusCode);
            }

            var fileContent = this.GetDecompressedContentFromHttpResponseMessage(restResponse);

            if (restResponse.ContentMD5 != null)
            {
                using (var crypto = MD5.Create())
                {
                    var hash = crypto.ComputeHash(fileContent);

                    if (!hash.SequenceEqual(restResponse.ContentMD5))
                    {
                        throw new RestException("Downloaded file failed hash check. Please try again");
                    }
                }
            }

            return new RestFile(content: fileContent, contentType: restResponse.RawContentType,
                contentHash: restResponse.ETag, contentLength: restResponse.Length, fileName: restResponse.FileName,
                statusCode: restResponse.StatusCode)
            {
                ContentMD5 = restResponse.ContentMD5
            };
        }

        public async Task<RestStreamResult> GetResponseStreamAsync(string url, RestCredentials credentials = null,
            CancellationToken? ctoken = null, object queryString = null, Dictionary<string, string> customHeaders = null)
        {
            var (_, response) = await this.ExecuteRequestAsync(url: url, credentials: credentials, method: HttpMethod.Get,
                userCancellationToken: ctoken, request: null, queryString: queryString, customHeaders: customHeaders)
                .ConfigureAwait(false);

            var contentLength = response.Content.Headers.ContentLength;

            var contentCompressionType = this.GetContentCompressionType(response.Content.Headers);

            var responseStream = await response.Content.ReadAsStreamAsync().ConfigureAwait(false);

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
                        customHeaders: customHeaders, httpContent: multipartFormDataContent)
                        .ConfigureAwait(false);
                }
            }
        }

        private HttpContent CreateCompressedJsonContent(object data)
        {
            return data == null
                ? null
                : new CompressedContent(
                    new StringContent(this.synchronizationSerializer.Serialize(data), Encoding.UTF8, "application/json"),
                    new GZipCompressor());
        }

        private async Task<T> ReceiveCompressedJsonWithProgressAsync<T>(ExecuteRequestResult response,
            CancellationToken token, IProgress<TransferProgress> transferProgress = null)
        {
            var restResponse = await this.ReceiveBytesWithProgressAsync(response, transferProgress, token).ConfigureAwait(false);

            return this.GetDecompressedJsonFromHttpResponseMessage<T>(restResponse);
        }

        private async Task<RestResponse> ReceiveBytesWithProgressAsync(ExecuteRequestResult result,
            IProgress<TransferProgress> transferProgress,
            CancellationToken token)
        {
            var responseMessage = result.Response;

            var restResponse = new RestResponse
            {
                ContentType = this.GetContentType(responseMessage.Content.Headers.ContentType?.MediaType),
                ContentCompressionType = this.GetContentCompressionType(responseMessage.Content.Headers),
                RawContentType = responseMessage.Content?.Headers?.ContentType?.MediaType,
                Length = responseMessage.Content?.Headers?.ContentLength,
                ETag = responseMessage.Headers?.ETag?.Tag,
                FileName = responseMessage.Content?.Headers?.ContentDisposition?.FileName,
                StatusCode = responseMessage.StatusCode
            };

            if (responseMessage.Content.Headers.ContentMD5 != null)
            {
                restResponse.ContentMD5 = responseMessage.Content.Headers.ContentMD5;
            }

            restResponse.Response = await fileDownloader.DownloadBinaryDataAsync(result.HttpClient, result.Response, transferProgress, token)
                .ConfigureAwait(false); ;

            return restResponse;
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
                throw new RestException(message: GenericSubdomains.Portable.Properties.Resources.CheckServerSettings, statusCode: HttpStatusCode.Redirect);

            try
            {
                var responseContent = GetDecompressedContentFromHttpResponseMessage(restResponse);
                // Debug.WriteLine($"JSON: {System.Text.Encoding.UTF8.GetString(responseContent)}");
                return this.synchronizationSerializer.Deserialize<T>(responseContent);
            }
            catch (JsonDeserializationException ex)
            {
                throw new RestException(message: GenericSubdomains.Portable.Properties.Resources.UpdateRequired, statusCode: HttpStatusCode.UpgradeRequired, innerException: ex);
            }
        }


        private byte[] GetDecompressedContentFromHttpResponseMessage(RestResponse restResponse)
        {
            var responseContent = restResponse.Response;
            return DecompressedContentFromHttpResponseMessage(restResponse.ContentCompressionType, responseContent);
        }

        private byte[] DecompressedContentFromHttpResponseMessage(RestContentCompressionType restContentCompressionType, byte[] responseContent)
        {
            switch (restContentCompressionType)
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
            public byte[] ContentMD5 { get; set; }
            public HttpStatusCode StatusCode { get; set; }
        }

        internal class ExecuteRequestResult
        {
            public ExecuteRequestResult(System.Net.Http.HttpClient httpClient, HttpResponseMessage httpResponseMessage)
            {
                Response = httpResponseMessage;
                HttpClient = httpClient;
            }

            public void Deconstruct(out System.Net.Http.HttpClient http, out HttpResponseMessage response)
            {
                http = HttpClient;
                response = Response;
            }

            public System.Net.Http.HttpClient HttpClient { get; }
            public HttpResponseMessage Response { get; }
        }

        internal enum RestContentType { Unknown, Json }
        internal enum RestContentCompressionType { None, GZip, Deflate }
    }
}
