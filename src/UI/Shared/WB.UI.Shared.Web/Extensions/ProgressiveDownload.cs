using System;
using System.IO;
using System.Net;
using System.Net.Http;
using System.Net.Http.Headers;
using WB.UI.Shared.Web.Filters;

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

        public bool IsRangeRequest => this.request.Headers.Range != null;

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
                var mediaTypeHeaderValue = MediaTypeHeaderValue.Parse(mediaType);
                if (IsRangeRequest)
                {
                    var response = this.request.CreateResponse(HttpStatusCode.PartialContent);
                    response.Content = new ByteRangeStreamContent(stream, this.request.Headers.Range, mediaTypeHeaderValue);

                    return response;
                }
                else
                {
                    var content = new StreamContent(stream);
                    var response = this.request.CreateResponse(HttpStatusCode.OK);
                    response.Content = content;
                    response.Content.Headers.ContentType = mediaTypeHeaderValue;

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
