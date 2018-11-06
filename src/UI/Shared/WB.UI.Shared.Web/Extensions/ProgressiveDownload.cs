using System;
using System.IO;
using System.Net;
using System.Net.Http;
using System.Net.Http.Headers;

namespace WB.UI.Shared.Web.Extensions
{
    /// <summary>
    /// Taken from https://github.com/ErikNoren/WebApi.ProgressiveDownloads/blob/master/VikingErik.Net.Http.ProgressiveDownload/ProgressiveDownload.cs
    /// </summary>
    public class ProgressiveDownload
    {
        private readonly HttpRequestMessage request;

        public ProgressiveDownload(HttpRequestMessage request)
        {
            this.request = request;
        }

        public bool IsRangeRequest => this.request.Headers.Range != null &&
                                      this.request.Headers.Range.Ranges.Count > 0;

        public HttpResponseMessage HeaderInfoMessage(long contentLength, string mediaType)
        {
            var response = this.request.CreateResponse();
            response.Content = new ByteArrayContent(Array.Empty<byte>());
            response.Content.Headers.ContentType = MediaTypeHeaderValue.Parse(mediaType);
            response.Content.Headers.ContentLength = contentLength;
            response.Headers.AcceptRanges.Add("bytes");
            return response;
        }

        public HttpResponseMessage ResultMessage(Stream stream, string mediaType)
        {
            try
            {
                if (IsRangeRequest)
                {
                    var content = new ByteRangeStreamContent(stream, this.request.Headers.Range, mediaType, 16 * 1024);
                    var response = this.request.CreateResponse(HttpStatusCode.PartialContent);
                    response.Headers.AcceptRanges.Add("bytes");
                    response.Content = content;

                    return response;
                }
                else
                {
                    var content = new StreamContent(stream);
                    var response = this.request.CreateResponse(HttpStatusCode.OK);
                    response.Headers.AcceptRanges.Add("bytes");
                    response.Content = content;
                    response.Content.Headers.ContentType = MediaTypeHeaderValue.Parse(mediaType);

                    return response;
                }
            }
            catch (InvalidByteRangeException ibr)
            {
                return this.request.CreateErrorResponse(ibr);
            }
        }
    }
}
