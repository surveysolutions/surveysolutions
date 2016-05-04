using System;
using System.Collections.ObjectModel;
using System.IO;
using System.Linq;
using System.Net.Http;
using System.Threading.Tasks;
using WB.Core.GenericSubdomains.Portable.Services;

namespace WB.Core.GenericSubdomains.Portable.Implementation.Services
{
    public class DecompressionHandler : DelegatingHandler
    {
        public Collection<ICompressor> Compressors;

        public DecompressionHandler()
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
                    request.Content = await DecompressContentAsync(request.Content, compressor).ConfigureAwait(true);
                }
            }

            var response = await base.SendAsync(request, cancellationToken).ConfigureAwait(true);

            return response;
        }

        private static async Task<HttpContent> DecompressContentAsync(HttpContent compressedContent, ICompressor compressor)
        {
            using (compressedContent)
            {
                var decompressed = new MemoryStream();
                await compressor.Decompress(await compressedContent.ReadAsStreamAsync(), decompressed).ConfigureAwait(true);

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