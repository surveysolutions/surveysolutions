using System;
using System.Collections.ObjectModel;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;
using WB.Core.GenericSubdomains.Portable.Implementation.Compression;
using WB.Core.GenericSubdomains.Portable.Services;

namespace WB.UI.Shared.Web.Compression
{
    /// <summary>
    /// Backported from https://github.com/azzlack/Microsoft.AspNet.WebApi.MessageHandlers.Compression
    /// </summary>
    public class CompressionHandler : DelegatingHandler
    {
        public readonly Collection<ICompressor> Compressors;

        public CompressionHandler()
        {
            this.Compressors = new Collection<ICompressor> { new GZipCompressor(), new DeflateCompressor() };
        }

        protected override async Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, System.Threading.CancellationToken cancellationToken)
        {
            if (request.Content.Headers.ContentEncoding?.Count > 0  && request.Content != null)
            {
                var encoding = request.Content.Headers.ContentEncoding.First();

                var compressor = this.Compressors.FirstOrDefault(c => c.EncodingType.Equals(encoding, StringComparison.OrdinalIgnoreCase));

                if (compressor != null)
                {
                    request.Content = await DecompressContentAsync(request.Content, compressor);
                }
            }

            if (cancellationToken.IsCancellationRequested)
            {
                return new HttpResponseMessage(HttpStatusCode.InternalServerError);
            }

            var response = await base.SendAsync(request, cancellationToken);

            response = await this.HandleCompression(request, response, cancellationToken);

            return response;
        }

        /// <summary>Handles the compression if applicable.</summary>
        /// <param name="request">The request.</param>
        /// <param name="response">The response.</param>
        /// <param name="cancellationToken">A cancellation token to cancel operation.</param>
        /// <returns>The response, compressed if applicable.</returns>
        public virtual async Task<HttpResponseMessage> HandleCompression(HttpRequestMessage request, HttpResponseMessage response, CancellationToken cancellationToken)
        {
            // Check if response should be compressed
            // NOTE: This must be done _after_ the response has been created, otherwise the EnableCompression property is not set
            var process = request.Headers.AcceptEncoding.Any(y => y.Value == "gzip" || y.Value == "deflate") &&
                          (response.Headers.AcceptRanges == null || response.Headers.AcceptRanges.Count == 0);

            // Compress content if it should processed
            if (process && response.Content != null)
            {
                try
                {
                    // Buffer content for further processing if content length is not known
                    if (!this.ContentLengthKnown(response))
                    {
                        await response.Content.LoadIntoBufferAsync();
                    }

                    this.CompressResponse(request, response);
                }
                catch (ObjectDisposedException)
                {
                   // Trace.TraceError($"Could not compress request, as response.Content had already been disposed: {x.Message}");
                }
            }

            return response;
        }

        /// <summary>Checks if the response content length is known.</summary>
        /// <param name="response">The response.</param>
        /// <returns>true if it is known, false if it it is not.</returns>
        private bool ContentLengthKnown(HttpResponseMessage response)
        {
            if (response?.Content == null)
            {
                return false;
            }

            if (response.Content is StringContent)
            {
                return true;
            }

            if (response.Content is ByteArrayContent)
            {
                return true;
            }

            if (response.Content is StreamContent)
            {
                return true;
            }

            return false;
        }

        private readonly int contentSizeThreshold = 1024; // do not compress content less then 1 Kb
        private readonly int contentMaxSizeThreshold = 100 * 1024 * 1024; // do not compress content more then 100 Mb

        private void CompressResponse(HttpRequestMessage request, HttpResponseMessage response)
        {
            // As per RFC2616.14.3:
            // Ignores encodings with quality == 0
            // If multiple content-codings are acceptable, then the acceptable content-coding with the highest non-zero qvalue is preferred.
            var compressor = (from encoding in request.Headers.AcceptEncoding
                let quality = encoding.Quality ?? 1.0
                where quality > 0
                join c in this.Compressors on encoding.Value.ToLowerInvariant() equals
                c.EncodingType.ToLowerInvariant()
                orderby quality descending
                select c).FirstOrDefault();

            if (compressor == null) return;

            try
            {
                // Only compress response if not already compressed
                if (response.Content?.Headers.ContentEncoding != null
                    && response.Content.Headers.ContentEncoding.Contains(compressor.EncodingType))
                {
                    return;
                }

                // Only compress response if size is larger than treshold (if set)
                if (this.contentSizeThreshold == 0)
                {
                    response.Content = new CompressedContent(response.Content, compressor);
                }
                else if (this.contentSizeThreshold > 0 
                    && response.Content.Headers.ContentLength >= this.contentSizeThreshold
                    && response.Content.Headers.ContentLength <= this.contentMaxSizeThreshold)
                {
                    response.Content = new CompressedContent(response.Content, compressor);
                }
            }
            catch (Exception ex)
            {
                throw new Exception($"Unable to compress response using compressor '{compressor.GetType()}'", ex);
            }
        }

        private static async Task<HttpContent> DecompressContentAsync(HttpContent compressedContent, ICompressor compressor)
        {
            using (compressedContent)
            {
                var decompressed = new MemoryStream();
                await compressor.Decompress(await compressedContent.ReadAsStreamAsync(), decompressed);

                // set position back to 0 so it can be read again
                decompressed.Position = 0;

                var newContent = new StreamContent(decompressed);
                // copy content type so we know how to load correct formatter
                newContent.Headers.ContentType = compressedContent.Headers.ContentType;
                return newContent;
            }
        }
    }
}
