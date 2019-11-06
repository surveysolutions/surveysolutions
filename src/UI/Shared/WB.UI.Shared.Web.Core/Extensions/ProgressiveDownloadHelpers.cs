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
    }
}
