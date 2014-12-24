using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Threading.Tasks;
using Microsoft.Practices.ServiceLocation;
using Newtonsoft.Json;
using WB.Core.GenericSubdomains.Utils.Services;

namespace WB.Core.GenericSubdomains.Utils.Implementation
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

        private static ILocalizationService localizationService
        {
            get { return ServiceLocator.Current.GetInstance<ILocalizationService>(); }
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
                    throw new RestException(message: localizationService.GetString("UpdateRequired"),
                        statusCode: HttpStatusCode.UpgradeRequired, innerException: ex);
                }
            }


            throw new RestException(message: localizationService.GetString("CheckServerSettings"), statusCode: HttpStatusCode.Redirect);
        }
    }
}