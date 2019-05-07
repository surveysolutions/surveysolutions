using System.IO;
using System.Net.Http;
using System.Web.Http;

namespace WB.UI.Shared.Web.Extensions
{
    public static class ProgressiveDownloadHelpers
    {
        public static HttpResponseMessage AsProgressiveDownload(this ApiController controller, Stream stream,
            string mediaType, string fileName = null, byte[] hash = null)
        {
            return new ProgressiveDownload(controller.Request).ResultMessage(stream, mediaType, fileName, hash);
        }

        public static HttpResponseMessage AsProgressiveDownload(this ApiController controller, byte[] data,
            string mediaType, string fileName = null, byte[] hash = null)
        {
            return new ProgressiveDownload(controller.Request).ResultMessage(new MemoryStream(data), mediaType, fileName, hash);
        }
    }
}
