using System;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Net.Http.Headers;
using WB.UI.Shared.Web.Exceptions;

namespace WB.UI.Shared.Web.Extensions
{
    public class ProgressiveDownload
    {
        private readonly HttpRequestMessage request;

        public ProgressiveDownload(HttpRequestMessage request)
        {
            this.request = request;
        }

        public HttpResponseMessage HeaderInfoMessage(long contentLength, string mediaType)
        {
            var response = new HttpResponseMessage();
            response.Content = new ByteArrayContent(Array.Empty<byte>());
            response.Content.Headers.ContentType = MediaTypeHeaderValue.Parse(mediaType);
            response.Content.Headers.ContentLength = contentLength;
            response.Headers.AcceptRanges.Add("bytes");
            return response;
        }

        public HttpResponseMessage ResultMessage(Stream stream, string mediaType, string fileName = null, byte[] hash = null)
        {
            var rangeHeader = this.request.Headers.Range?.Ranges.FirstOrDefault();
            if (rangeHeader != null)
            {
                var byteRange = new ByteRangeStreamContent(stream, this.request.Headers.Range, mediaType);
                var partialResponse = new HttpResponseMessage(HttpStatusCode.PartialContent);

                partialResponse.Content = new PushStreamContent((outputStream, content, context) =>
                {
                    try
                    {
                        stream.Seek(rangeHeader.From ?? 0, SeekOrigin.Begin);

                        long bufferSize = 1024 * 1024;
                        byte[] buffer = new byte[bufferSize];

                        while (true)
                        {
                            if (rangeHeader.To != null)
                            {
                                bufferSize = Math.Min(buffer.Length, rangeHeader.To.Value - stream.Position + 1);
                            }

                            if (bufferSize == 0) return;

                            int count = stream.Read(buffer, 0, (int)bufferSize);
                            if (count != 0)
                            {
                                outputStream.Write(buffer, 0, count);
                                outputStream.Flush();
                            }
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

                if (!string.IsNullOrEmpty(fileName))
                {
                    partialResponse.Content.Headers.ContentDisposition = new ContentDispositionHeaderValue("attachment")
                    {
                        FileName = fileName
                    };
                }

                if (hash != null)
                {
                    partialResponse.Content.Headers.ContentMD5 = hash;
                }

                partialResponse.Content.Headers.ContentType = byteRange.Headers.ContentType;
                partialResponse.Content.Headers.ContentLength = byteRange.Headers.ContentLength;
                partialResponse.Content.Headers.ContentRange = byteRange.Headers.ContentRange;
                partialResponse.Headers.AcceptRanges.Add("bytes");
                return partialResponse;
            }

            var nonPartialResponse = this.request.CreateResponse(HttpStatusCode.OK);

            nonPartialResponse.Content = new StreamContent(stream);

            if (hash != null)
            {
                nonPartialResponse.Content.Headers.ContentMD5 = hash;
            }

            nonPartialResponse.Content.Headers.ContentType = new MediaTypeHeaderValue(mediaType);
            nonPartialResponse.Headers.AcceptRanges.Add("bytes");

            if (!string.IsNullOrEmpty(fileName))
            {
                nonPartialResponse.Content.Headers.ContentDisposition = new ContentDispositionHeaderValue("attachment")
                {
                    FileName = fileName
                };
            }
            return nonPartialResponse;
        }
    }
}
