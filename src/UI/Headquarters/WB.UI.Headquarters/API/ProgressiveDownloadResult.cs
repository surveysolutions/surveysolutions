using System.IO;
using System.Net;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Threading;
using System.Threading.Tasks;
using System.Web;
using System.Web.Http;

namespace WB.UI.Headquarters.API
{
    public class ProgressiveDownloadResult : IHttpActionResult
    {
        private readonly HttpRequestMessage request;
        private readonly string filePath;
        private readonly string fileName;
        private readonly string mediaType;

        private bool isRangeRequest => this.request.Headers.Range != null &&
                                       this.request.Headers.Range.Ranges.Count > 0;

        public ProgressiveDownloadResult(HttpRequestMessage request, string filePath, string fileName, string mediaType)
        {
            this.request = request;
            this.filePath = filePath;
            this.fileName = fileName;
            this.mediaType = mediaType;
        }

        Task<HttpResponseMessage> IHttpActionResult.ExecuteAsync(CancellationToken cancellationToken)
        {
            var fileStream = new FileStream(this.filePath, FileMode.Open, FileAccess.Read);

            HttpResponseMessage response;
            try
            {
                if (this.isRangeRequest)
                {
                    response = this.request.CreateResponse(HttpStatusCode.PartialContent);
                    response.Content = new ByteRangeStreamContent(fileStream, this.request.Headers.Range, mediaType);
                }
                else
                {
                    response = this.request.CreateResponse(HttpStatusCode.OK);
                    response.Content = new StreamContent(fileStream);
                    response.Content.Headers.ContentType = MediaTypeHeaderValue.Parse(mediaType);
                }
            }
            catch (InvalidByteRangeException ibr)
            {
                response = this.request.CreateErrorResponse(ibr);
            }

            response.Headers.AcceptRanges.Add(@"bytes");
            response.Content.Headers.ContentDisposition = new ContentDispositionHeaderValue(@"attachment")
            {
                FileNameStar = HttpUtility.UrlEncode(this.fileName)
            };

            return Task.FromResult(response);
        }
    }
}