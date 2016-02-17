﻿using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Flurl;
using Flurl.Http;
using Flurl.Http.Content;
using WB.Core.GenericSubdomains.Portable.Properties;
using WB.Core.GenericSubdomains.Portable.Services;

namespace WB.Core.GenericSubdomains.Portable.Implementation.Services
{
    public class RestService : IRestService
    {
        private readonly IRestServiceSettings restServiceSettings;
        private readonly INetworkService networkService;
        private readonly ISerializer serializer;
        private readonly IStringCompressor stringCompressor;

        public RestService(
            IRestServiceSettings restServiceSettings, 
            INetworkService networkService,
            ISerializer serializer,
            IStringCompressor stringCompressor,
            IRestServicePointManager restServicePointManager)
        {
            this.restServiceSettings = restServiceSettings;
            this.networkService = networkService;
            this.serializer = serializer;
            this.stringCompressor = stringCompressor;

            if (this.restServiceSettings.AcceptUnsignedSslCertificate && restServicePointManager != null)
                restServicePointManager.AcceptUnsignedSslCertificate();
        }

        private async Task<HttpResponseMessage> ExecuteRequestAsync(
            string url, 
            HttpMethod method,
            object queryString = null, 
            object request = null,
            RestCredentials credentials = null,
            CancellationToken? userCancellationToken = null)
        {
            if (!this.IsValidHostAddress(this.restServiceSettings.Endpoint))
                throw new RestException("Invalid URL", type: RestExceptionType.InvalidUrl);

            if (this.networkService != null)
            {
                if (!this.networkService.IsNetworkEnabled())
                    throw new RestException("No network", type: RestExceptionType.NoNetwork);

                if(!this.networkService.IsHostReachable(this.restServiceSettings.Endpoint))
                    throw new RestException("Host unreachable", type: RestExceptionType.HostUnreachable);
            }

            var requestTimeoutToken = new CancellationTokenSource(this.restServiceSettings.Timeout).Token;
            var linkedCancellationTokenSource = CancellationTokenSource.CreateLinkedTokenSource(requestTimeoutToken,
                userCancellationToken ?? default(CancellationToken));

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
                return await restClient.SendAsync(method, this.CreateJsonContent(request),
                    linkedCancellationTokenSource.Token);
            }
            catch (OperationCanceledException ex)
            {
                // throwed when receiving bytes in ReceiveBytesWithProgressAsync method and user canceling request
                throw new RestException("Request canceled by user", type: RestExceptionType.RequestCanceledByUser, innerException: ex); 
            }
            catch (FlurlHttpException ex)
            {
                if (ex.GetSelfOrInnerAs<TaskCanceledException>() != null)
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
                    throw new RestException(ex.Call.Response.ReasonPhrase, statusCode: ex.Call.Response.StatusCode,
                           innerException: ex);   
                }

                throw new RestException(message: "Unexpected web exception", innerException: ex);
            }
        }

        public async Task GetAsync(string url, object queryString = null, RestCredentials credentials = null, CancellationToken? token = null)
        {
            await this.ExecuteRequestAsync(url: url, queryString: queryString, credentials: credentials,
                    method: HttpMethod.Get, userCancellationToken: token);
        }

        public async Task PostAsync(string url, object request = null, RestCredentials credentials = null,
            CancellationToken? token = null)
        {
            await this.ExecuteRequestAsync(url: url, credentials: credentials, method: HttpMethod.Post, request: request,
                userCancellationToken: token);
        }

        public async Task<T> GetAsync<T>(string url,
            Action<DownloadProgressChangedEventArgs> onDownloadProgressChanged, object queryString = null,
            RestCredentials credentials = null, CancellationToken? token = null)
        {
            var response = this.ExecuteRequestAsync(url: url, queryString: queryString, credentials: credentials, method: HttpMethod.Get,
                userCancellationToken: token);

            return await this.ReceiveCompressedJsonWithProgressAsync<T>(response: response, token: token ?? default(CancellationToken),
                onDownloadProgressChanged: onDownloadProgressChanged);
        }

        public async Task<T> PostAsync<T>(string url,
            Action<DownloadProgressChangedEventArgs> onDownloadProgressChanged, object request = null,
            RestCredentials credentials = null, CancellationToken? token = null)
        {
            var response = this.ExecuteRequestAsync(url: url, credentials: credentials, method: HttpMethod.Post, request: request,
                userCancellationToken: token);

            return await this.ReceiveCompressedJsonWithProgressAsync<T>(response: response, token: token ?? default(CancellationToken),
                onDownloadProgressChanged: onDownloadProgressChanged);
        }

        public async Task<byte[]> DownloadFileAsync(string url,
            Action<DownloadProgressChangedEventArgs> onDownloadProgressChanged, RestCredentials credentials = null,
            CancellationToken? token = null)
        {
            var response = this.ExecuteRequestAsync(url: url, credentials: credentials, method: HttpMethod.Get,
                userCancellationToken: token);

            var restResponse = await this.ReceiveBytesWithProgressAsync(response: response, token: token ?? default(CancellationToken),
                        onDownloadProgressChanged: onDownloadProgressChanged);

            var fileContent = this.GetDecompressedContentFromHttpResponseMessage(restResponse);
            return fileContent;
        }

        private HttpContent CreateJsonContent(object data)
        {
            return data == null ? null : new CapturedStringContent(this.serializer.Serialize(data), Encoding.UTF8, "application/json");
        }

        private async Task<T> ReceiveCompressedJsonWithProgressAsync<T>(Task<HttpResponseMessage> response,
            CancellationToken token, Action<DownloadProgressChangedEventArgs> onDownloadProgressChanged = null)
        {
            var restResponse = await this.ReceiveBytesWithProgressAsync(token, onDownloadProgressChanged, response);

            return this.GetDecompressedJsonFromHttpResponseMessage<T>(restResponse);
        }

        private async Task<RestResponse> ReceiveBytesWithProgressAsync(CancellationToken token, Action<DownloadProgressChangedEventArgs> onDownloadProgressChanged,
            Task<HttpResponseMessage> response)
        {
            var responseMessage = await response;
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

            return new RestResponse()
            {
                Response = responseContent,
                ContentType = this.GetContentType(responseMessage.Content.Headers.ContentType.MediaType),
                ContentCompressionType = this.GetContentCompressionType(responseMessage.Content.Headers)
            };
        }

        private RestContentType GetContentType(string mediaType)
        {
            if(mediaType.IndexOf("json", StringComparison.OrdinalIgnoreCase) > -1 || mediaType.IndexOf("javascript", StringComparison.OrdinalIgnoreCase) > -1)
                return RestContentType.Json;

            return RestContentType.Unknown;
        }

        private RestContentCompressionType GetContentCompressionType(HttpContentHeaders headers)
        {
            IEnumerable<string> acceptedEncodings;
            headers.TryGetValues("Content-Encoding", out acceptedEncodings);

            if (acceptedEncodings == null) return RestContentCompressionType.None;

            if(acceptedEncodings.Contains("gzip"))
                return RestContentCompressionType.GZip;

            if(acceptedEncodings.Contains("deflate"))
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
                return this.serializer.Deserialize<T>(responseContent);
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

        private bool IsValidHostAddress(string url)
        {
            Uri uriResult;
            return Uri.TryCreate(url, UriKind.Absolute, out uriResult) && (uriResult.Scheme == "http" || uriResult.Scheme == "https");
        }

        internal class RestResponse
        {
            public byte[] Response { get; set; }
            public RestContentType ContentType { get; set; }
            public RestContentCompressionType ContentCompressionType { get; set; }
        }

        internal enum RestContentType{ Unknown, Json }
        internal enum RestContentCompressionType { None, GZip, Deflate }
    }
}
