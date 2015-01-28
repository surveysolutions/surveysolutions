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
using WB.Core.GenericSubdomains.Utils.Implementation;
using WB.Core.GenericSubdomains.Utils.Rest.Properties;
using WB.Core.GenericSubdomains.Utils.Services;

namespace WB.Core.GenericSubdomains.Utils.Rest
{
    public static class RestExtensions
    {
        private static IStringCompressor stringCompressor
        {
            get { return ServiceLocator.Current.GetInstance<IStringCompressor>(); }
        }

        private static IJsonUtils jsonUtils
        {
            get { return ServiceLocator.Current.GetInstance<IJsonUtils>(); }
        }

        public static async Task<T> ReceiveCompressedJson<T>(this Task<HttpResponseMessage> response)
        {
            var responseMessage = await response;

            var responseContent = await responseMessage.Content.ReadAsByteArrayAsync();

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
                catch (JsonReaderException ex)
                {
                    throw new RestException(message: Resources.UpdateRequired,
                        statusCode: HttpStatusCode.UpgradeRequired, innerException: ex);
                }
            }


            throw new RestException(message: Resources.CheckServerSettings, statusCode: HttpStatusCode.Redirect);
        }

        public static async Task<T> GetJsonAsyncWithProgress<T>(this Task<HttpResponseMessage> response, CancellationToken token, IProgress<int> progress = null)
        {
            var responseMessage = await response;
            //var a = responseMessage.Content.Headers.ContentLength;
            byte[] responseContent;

            using (var responseStream = await responseMessage.Content.ReadAsStreamAsync())
            {

                if (token.IsCancellationRequested)
                {
                    token.ThrowIfCancellationRequested();
                }

                byte[] buffer = new byte[16*1024];
                using (var ms = new MemoryStream())
                {
                    int read;
                    while ((read = await responseStream.ReadAsync(buffer, 0, buffer.Length, token)) > 0)
                    {
                        ms.Write(buffer, 0, read);
                        progress.Report((int) ms.Length);
                    }
                    responseContent = ms.ToArray();
                }
            }

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
                catch (JsonReaderException ex)
                {
                    throw new RestException(message: Resources.UpdateRequired,
                        statusCode: HttpStatusCode.UpgradeRequired, innerException: ex);
                }
            }


            throw new RestException(message: Resources.CheckServerSettings, statusCode: HttpStatusCode.Redirect);
        }
    }
}