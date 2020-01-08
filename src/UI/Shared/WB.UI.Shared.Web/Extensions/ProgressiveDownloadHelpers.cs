using System;
using System.IO;
using System.Net.Http;

namespace WB.UI.Shared.Web.Extensions
{
    public static class ProgressiveDownloadHelpers
    {
        public static HttpResponseMessage AsProgressiveDownload(this HttpRequestMessage request, Stream stream,
            string mediaType, string fileName = null, byte[] hash = null)
        {
            return new ProgressiveDownload(request).ResultMessage(stream, mediaType, fileName, hash);
        }

        public static HttpResponseMessage AsProgressiveDownload(this HttpRequestMessage request, byte[] data,
            string mediaType, string fileName = null, byte[] hash = null)
        {
            return new ProgressiveDownload(request).ResultMessage(new MemoryStream(data), mediaType, fileName, hash);
        }

        public static bool InterviewerClientSupportProgressiveDownload(this HttpRequestMessage request)
        {
            if (!KP_13477_Tablets_cannot_update(request)) return false;

            return true;
        }

        // This method should not be changed. Add new condition with new method
        private static bool KP_13477_Tablets_cannot_update(HttpRequestMessage request)
        {
            // KP-13477 Tablets cannot update
            var clientVersion = request.GetProductVersionFromUserAgent(@"org.worldbank.solutions.interviewer")
                                ?? request.GetProductVersionFromUserAgent(@"org.worldbank.solutions.supervisor");

            if (clientVersion == null) return true;

            return clientVersion > new Version(19, 12, 3); // 19.12.3 (build 26644) 
        }

        public static Version GetProductVersionFromUserAgent(this HttpRequestMessage request, string productName)
        {
            if (request?.Headers?.UserAgent == null) return null;

            foreach (var product in request.Headers?.UserAgent)
            {
                if ((product.Product?.Name.StartsWith(productName, StringComparison.OrdinalIgnoreCase) ?? false)
                    && Version.TryParse(product.Product.Version, out Version version))
                {
                    return version;
                }
            }

            return null;
        }

        public static int? GetBuildNumberFromUserAgent(this HttpRequestMessage request)
        {
            if (request?.Headers?.UserAgent == null) return null;

            foreach (var product in request.Headers?.UserAgent)
            {
                if ((product.Comment?.Contains("build") ?? false) 
                    && int.TryParse(product.Comment.Split(' ')[1].Replace(")", ""), out int build))
                {
                    return build;
                }
            }

            return null;
        }
    }
}
