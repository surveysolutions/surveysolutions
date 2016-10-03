using System;
using System.Collections.Generic;
using System.IO;
using System.IO.Compression;
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
using Newtonsoft.Json;

namespace ApiUtil
{
    public class DownloadProgressChangedEventArgs
    {
        public long? TotalBytesToReceive { get; set; }
        public long BytesReceived { get; set; }
        public decimal ProgressPercentage { get; set; }
    }

    internal enum RestContentType { Unknown, Json }
    internal enum RestContentCompressionType { None, GZip, Deflate }

    public class JsonDeserializationException : Exception
    {
        public JsonDeserializationException(string message, Exception innerException = null)
            : base(message, innerException)
        {
        }
    }

    public class RestFile
    {
        public RestFile(byte[] content, string contentType, string contentHash, long? contentLength, string fileName)
        {
            this.Content = content;
            this.ContentType = contentType;
            this.ContentHash = contentHash;
            this.ContentLength = contentLength;
            this.FileName = fileName;
        }

        private string FileName { get; }
        public string ContentHash { get; }
        public long? ContentLength { get; }
        public string ContentType { get; }
        public byte[] Content { get; }
    }

    internal class RestResponse
    {
        public byte[] Response { get; set; }
        public RestContentType ContentType { get; set; }
        public RestContentCompressionType ContentCompressionType { get; set; }
        public string RawContentType { get; set; }
        public long? Length { get; set; }
        public string ETag { get; set; }
        public string FileName { get; set; }
    }

    internal class Rest
    {
        public static async Task<RestFile> DownloadFileAsync(string endpointUrl, string relativeUrl, ConsoleRestServiceSettings restServiceSettings,
             int retriesPerOperation,
           Action<DownloadProgressChangedEventArgs> onDownloadProgressChanged = null, RestCredentials credentials = null,
           CancellationToken? token = null)
        {
            var response = ExecuteRequestAsync(
                endpointUrl: endpointUrl, 
                relativeUrl: relativeUrl, 
                restServiceSettings: restServiceSettings,
                retriesPerOperation: retriesPerOperation,
                credentials: credentials, 
                method: HttpMethod.Get,
                token: token,
                request: null);

            var restResponse = await ReceiveBytesWithProgressAsync(response: response, token: token ?? default(CancellationToken),
                        onDownloadProgressChanged: onDownloadProgressChanged);

            var fileContent = GetDecompressedContentFromHttpResponseMessage(restResponse);

            return new RestFile(content: fileContent, contentType: restResponse.RawContentType,
                contentHash: restResponse.ETag, contentLength: restResponse.Length, fileName: restResponse.FileName);
        }

        public static async Task<T> GetAsync<T>(string endpointUrl, string relativeUrl, ConsoleRestServiceSettings restServiceSettings,
             int retriesPerOperation,
            Action<DownloadProgressChangedEventArgs> onDownloadProgressChanged = null, object queryString = null,
            RestCredentials credentials = null, CancellationToken? token = null)
        {
            var response = ExecuteRequestAsync(
                endpointUrl: endpointUrl,
                relativeUrl: relativeUrl,
                queryString: queryString,
                restServiceSettings: restServiceSettings,
                retriesPerOperation: retriesPerOperation,
                credentials: credentials, 
                method: HttpMethod.Get,
                token: token, 
                request: null);

            return await ReceiveCompressedJsonWithProgressAsync<T>(response: response, token: token ?? default(CancellationToken),
                onDownloadProgressChanged: onDownloadProgressChanged);
        }

        public async Task<T> PostAsync<T>(string endpointUrl, string relativeUrl, ConsoleRestServiceSettings restServiceSettings,
             int retriesPerOperation,
            Action<DownloadProgressChangedEventArgs> onDownloadProgressChanged = null, object request = null,
            RestCredentials credentials = null, CancellationToken? token = null)
        {
            var response = ExecuteRequestAsync(
                endpointUrl: endpointUrl, 
                relativeUrl: relativeUrl, 
                method: HttpMethod.Post, 
                restServiceSettings: restServiceSettings, 
                retriesPerOperation: retriesPerOperation,
                credentials: credentials, 
                request: request,
                token: token);

            return await ReceiveCompressedJsonWithProgressAsync<T>(response: response, token: token ?? default(CancellationToken),
                onDownloadProgressChanged: onDownloadProgressChanged);
        }

        public static async Task<HttpResponseMessage> ExecuteRequestAsync(
            string endpointUrl, 
            string relativeUrl, 
            HttpMethod method, 
            ConsoleRestServiceSettings restServiceSettings, 
            int retriesPerOperation, 
            RestCredentials credentials = null, 
            object queryString = null, 
            object request = null, 
            CancellationToken? token = null)
        {
            var requestTimeoutToken = new CancellationTokenSource(restServiceSettings.Timeout).Token;
            var linkedCancellationTokenSource = CancellationTokenSource.CreateLinkedTokenSource(requestTimeoutToken, token ?? default(CancellationToken));

            var fullUrl = endpointUrl
                .AppendPathSegment(relativeUrl)
                .SetQueryParams(queryString);

            var restClient = fullUrl
                .WithTimeout(restServiceSettings.Timeout)
                .WithHeader("Accept-Encoding", "gzip,deflate");

            if (credentials != null)
                restClient.WithBasicAuth(credentials.Login, credentials.Password);

            HttpResponseMessage result = null;
            var httpContent = CreateJsonContent(request);
            var retriesLeft = retriesPerOperation;

            while (retriesLeft > 0)
            {
                try
                {
                    result = await restClient.SendAsync(method, httpContent, linkedCancellationTokenSource.Token);
                    Console.WriteLine("Request has been sent successfully.");
                    return result;
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
                        if (retriesLeft > 1) // ignoring all statuses except AR errors
                        {
                            retriesLeft--;
                            continue;
                        }

                        if (requestTimeoutToken.IsCancellationRequested)
                        {
                            throw new RestException("Request timeout", type: RestExceptionType.RequestByTimeout, statusCode: HttpStatusCode.RequestTimeout, innerException: ex);
                        }

                        if (token.HasValue && token.Value.IsCancellationRequested)
                        {
                            throw new RestException("Request canceled by user", type: RestExceptionType.RequestCanceledByUser, innerException: ex);
                        }
                    }
                    else if (ex.Call.Response != null)
                    {

                        if (retriesLeft > 1 && ex.Call.Response.StatusCode == HttpStatusCode.NotAcceptable) // ignoring all statuses except AR errors 
                        {
                            retriesLeft--;
                            continue;
                        }

                        //invalid status 406 should be thrown on first round
                        throw new RestException(ex.Call.ErrorResponseBody ?? ex.Call.Response.ReasonPhrase, statusCode: ex.Call.Response.StatusCode, innerException: ex);
                    }

                    throw new RestException(message: "Unexpected web exception", innerException: ex);
                }
                catch (Exception ex)
                {
                    throw new RestException(message: "Unexpected web exception", innerException: ex);
                }
            }

            return result;
        }

        private static async Task<T> ReceiveCompressedJsonWithProgressAsync<T>(Task<HttpResponseMessage> response,
           CancellationToken token, Action<DownloadProgressChangedEventArgs> onDownloadProgressChanged = null)
        {
            var restResponse = await ReceiveBytesWithProgressAsync(token, onDownloadProgressChanged, response);

            return GetDecompressedJsonFromHttpResponseMessage<T>(restResponse);
        }

        private static T GetDecompressedJsonFromHttpResponseMessage<T>(RestResponse restResponse)
        {
            if (restResponse.ContentType != RestContentType.Json)
                throw new RestException(message: "CheckServerSettings", statusCode: HttpStatusCode.Redirect);

            try
            {
                byte[] responseContent = GetDecompressedContentFromHttpResponseMessage(restResponse);
                return Deserialize<T>(responseContent);
            }
            catch (JsonDeserializationException ex)
            {
                throw new RestException(message: "UpdateRequired", statusCode: HttpStatusCode.UpgradeRequired, innerException: ex);
            }
        }

        public static T Deserialize<T>(byte[] payload)
        {
            try
            {
                var input = new MemoryStream(payload);
                using (var reader = new StreamReader(input))
                {
                    return JsonSerializer.Create(new JsonSerializerSettings
                    {
                        TypeNameHandling = TypeNameHandling.All,
                        NullValueHandling = NullValueHandling.Ignore,
                        FloatParseHandling = FloatParseHandling.Decimal
                    }).Deserialize<T>(new JsonTextReader(reader));
                }
            }
            catch (JsonReaderException ex)
            {
                throw new JsonDeserializationException(ex.Message, ex);
            }
        }

        private static byte[] GetDecompressedContentFromHttpResponseMessage(RestResponse restResponse)
        {
            var responseContent = restResponse.Response;

            switch (restResponse.ContentCompressionType)
            {
                case RestContentCompressionType.GZip:
                    responseContent = DecompressGZip(responseContent);
                    break;
                case RestContentCompressionType.Deflate:
                    responseContent = DecompressDeflate(responseContent);
                    break;
            }

            return responseContent;
        }

        public static byte[] DecompressGZip(byte[] payload)
        {
            using (var msi = new MemoryStream(payload))
            using (var mso = new MemoryStream())
            {
                using (var gs = new GZipStream(msi, CompressionMode.Decompress))
                {
                    gs.CopyTo(mso);
                }
                return mso.ToArray();
            }
        }

        public static byte[] DecompressDeflate(byte[] payload)
        {
            using (var msi = new MemoryStream(payload))
            using (var mso = new MemoryStream())
            {
                using (var gs = new DeflateStream(msi, CompressionMode.Decompress))
                {
                    gs.CopyTo(mso);
                }
                return mso.ToArray();
            }
        }

        private static async Task<RestResponse> ReceiveBytesWithProgressAsync(CancellationToken token, Action<DownloadProgressChangedEventArgs> onDownloadProgressChanged,
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

                var buffer = new byte[1024];
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
                ContentType = GetContentType(responseMessage.Content.Headers.ContentType.MediaType),
                ContentCompressionType = GetContentCompressionType(responseMessage.Content.Headers),
                RawContentType = responseMessage.Content?.Headers?.ContentType?.MediaType,
                Length = responseMessage.Content?.Headers?.ContentLength,
                ETag = responseMessage.Headers?.ETag?.Tag.Trim('"'),
                FileName = responseMessage.Content?.Headers?.ContentDisposition?.FileName
            };
        }

        private static RestContentType GetContentType(string mediaType)
        {
            if (mediaType.IndexOf("json", StringComparison.OrdinalIgnoreCase) > -1 || mediaType.IndexOf("javascript", StringComparison.OrdinalIgnoreCase) > -1)
                return RestContentType.Json;

            return RestContentType.Unknown;
        }

        private static RestContentCompressionType GetContentCompressionType(HttpContentHeaders headers)
        {
            IEnumerable<string> acceptedEncodings;
            headers.TryGetValues("Content-Encoding", out acceptedEncodings);

            if (acceptedEncodings == null) return RestContentCompressionType.None;

            if (acceptedEncodings.Contains("gzip"))
                return RestContentCompressionType.GZip;

            if (acceptedEncodings.Contains("deflate"))
                return RestContentCompressionType.Deflate;

            return RestContentCompressionType.None;
        }

        private static HttpContent CreateJsonContent(object data)
        {
            var serializedData = JsonConvert.SerializeObject(data);
            return data == null ? null : new CapturedStringContent(serializedData, Encoding.UTF8, "application/json");
        }
    }
}