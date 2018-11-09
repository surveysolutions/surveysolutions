using System;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Security.Authentication.ExtendedProtection;
using System.Web;
using System.Web.Mvc;
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
            var rangeHeader = this.request.Headers.Range?.Ranges.FirstOrDefault();
            if (rangeHeader != null)
            {
                var byteRange = new ByteRangeStreamContent(stream, this.request.Headers.Range, mediaType);
                var partialResponse = this.request.CreateResponse(HttpStatusCode.PartialContent);
                partialResponse.Content = new PushStreamContent((outputStream, content, context) =>
                {
                    try
                    {
                        stream.Seek(rangeHeader.From ?? 0, SeekOrigin.Begin);

                        long bufferSize = 32 * 1024;
                        byte[] buffer = new byte[bufferSize];

                        while (true)
                        {
                            if (rangeHeader.To != null)
                            {
                                bufferSize = Math.Min(bufferSize, rangeHeader.To.Value - stream.Position);
                            }

                            int count = stream.Read(buffer, 0, (int) bufferSize);
                            if (count != 0)
                                outputStream.Write(buffer, 0, count);
                            else
                                break;
                        }
                    }
                    catch (HttpException)
                    {
                    }
                    finally
                    {
                        outputStream.Close();
                    }
                });

                partialResponse.Content.Headers.ContentType = byteRange.Headers.ContentType;
                partialResponse.Content.Headers.ContentLength = byteRange.Headers.ContentLength;
                partialResponse.Content.Headers.ContentRange = byteRange.Headers.ContentRange;

                return partialResponse;
            }

            var nonPartialResponse = this.request.CreateResponse(HttpStatusCode.OK);

            nonPartialResponse.Content = new StreamContent(stream);
            nonPartialResponse.Content.Headers.ContentType = new MediaTypeHeaderValue(mediaType);
            return nonPartialResponse;
        }
    }
}
