using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Practices.ServiceLocation;
using Newtonsoft.Json;
using WB.Core.BoundedContexts.QuestionnaireTester.Infrastructure;

namespace WB.Core.Infrastructure.Android.Implementation.Services.Rest
{
    public static class RestExtensions
    {
        private static ICompressor Compressor
        {
            get { return ServiceLocator.Current.GetInstance<ICompressor>(); }
        }

        private static RestServiceSettings RestServiceSettings
        {
            get { return ServiceLocator.Current.GetInstance<RestServiceSettings>(); }
        }

        private static IJsonUtils JsonUtils
        {
            get { return ServiceLocator.Current.GetInstance<IJsonUtils>(); }
        }

        public static async Task<T> ReceiveCompressedJsonAsync<T>(this Task<HttpResponseMessage> response)
        {
            var responseMessage = await response;
            var responseContent = await responseMessage.Content.ReadAsByteArrayAsync();

            return await GetDecompressedJsonFromHttpResponseMessage<T>(responseMessage, responseContent);
        }


        public static async Task<T> ReceiveCompressedJsonWithProgressAsync<T>(this Task<HttpResponseMessage> response, CancellationToken token, Action<decimal> progressPercentage = null)
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

                var buffer = new byte[RestServiceSettings.BufferSize];
                using (var ms = new MemoryStream())
                {
                    int read;
                    while ((read = await responseStream.ReadAsync(buffer, 0, buffer.Length, token)) > 0)
                    {
                        ms.Write(buffer, 0, read);
                        if (progressPercentage != null)
                        {
                           progressPercentage(contentLength.HasValue ? Math.Round((decimal) (100*ms.Length)/contentLength.Value) : ms.Length);
                        }
                    }
                    responseContent = ms.ToArray();
                }
            }

            return await GetDecompressedJsonFromHttpResponseMessage<T>(responseMessage, responseContent);
        }

        private static async Task<T> GetDecompressedJsonFromHttpResponseMessage<T>(HttpResponseMessage responseMessage, byte[] responseContent)
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
                        responseContent = await Compressor.DecompressGZipAsync(responseContent);
                    }

                    if (acceptedEncodings.Contains("deflate"))
                    {
                        responseContent = await Compressor.DecompressDeflateAsync(responseContent);
                    }
                }

                try
                {
                    return await JsonUtils.DeserializeAsync<T>(responseContent);
                }
                catch (JsonReaderException ex)
                {
                    throw new RestException(message: "Could not deserialize response",
                        statusCode: HttpStatusCode.UpgradeRequired, innerException: ex);
                }
            }


            throw new RestException(message: "Response is not a JSON", statusCode: HttpStatusCode.Redirect);
        }
    }
}