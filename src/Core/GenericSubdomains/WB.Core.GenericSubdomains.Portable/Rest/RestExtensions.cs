using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Practices.ServiceLocation;
using WB.Core.GenericSubdomains.Portable.Implementation;
using WB.Core.GenericSubdomains.Portable.Properties;
using WB.Core.GenericSubdomains.Portable.Services;

namespace WB.Core.GenericSubdomains.Portable.Rest
{
    public static class RestExtensions
    {
        private static IStringCompressor stringCompressor
        {
            get { return ServiceLocator.Current.GetInstance<IStringCompressor>(); }
        }

        private static IRestServiceSettings restServiceSettings
        {
            get { return ServiceLocator.Current.GetInstance<IRestServiceSettings>(); }
        }

        private static IJsonUtils jsonUtils
        {
            get { return ServiceLocator.Current.GetInstance<IJsonUtils>(); }
        }

        public static async Task<T> ReceiveCompressedJsonAsync<T>(this Task<HttpResponseMessage> response)
        {
            var responseMessage = await response;
            var responseContent = await responseMessage.Content.ReadAsByteArrayAsync();

            return GetDecompressedJsonFromHttpResponseMessage<T>(responseMessage, responseContent);
        }


        public static async Task<T> ReceiveCompressedJsonWithProgressAsync<T>(this Task<HttpResponseMessage> response, CancellationToken token, Action<DownloadProgressChangedEventArgs> onDownloadProgressChanged = null)
        {
            var responseMessage = await response;

            var responseContent = await GetByteArrayResponseContentWithProgressAsync(token, onDownloadProgressChanged, responseMessage);

            return GetDecompressedJsonFromHttpResponseMessage<T>(responseMessage, responseContent);
        }

        public static async Task<byte[]> ReceiveBytesWithProgressAsync(this Task<HttpResponseMessage> response, CancellationToken token, Action<DownloadProgressChangedEventArgs> onDownloadProgressChanged = null)
        {
            var responseMessage = await response;

            return await GetByteArrayResponseContentWithProgressAsync(token, onDownloadProgressChanged, responseMessage);
        }

        private static async Task<byte[]> GetByteArrayResponseContentWithProgressAsync(CancellationToken token, Action<DownloadProgressChangedEventArgs> onDownloadProgressChanged,
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

                var buffer = new byte[restServiceSettings.BufferSize];
                var downloadProgressChangedEventArgs = new DownloadProgressChangedEventArgs()
                {
                    TotalBytesToReceive = contentLength.Value
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

        private static T GetDecompressedJsonFromHttpResponseMessage<T>(HttpResponseMessage responseMessage, byte[] responseContent)
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
                        responseContent = stringCompressor.DecompressGZip(responseContent);
                    }

                    if (acceptedEncodings.Contains("deflate"))
                    {
                        responseContent = stringCompressor.DecompressDeflate(responseContent);
                    }
                }

                try
                {
                    return jsonUtils.Deserialize<T>(responseContent);
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