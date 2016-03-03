using System;
using System.IO;
using System.Net;
using System.Net.Http;
using System.Threading.Tasks;
using WB.Core.GenericSubdomains.Portable.Services;

namespace WB.Core.GenericSubdomains.Portable.Implementation.Services
{
    public class CompressedContent : HttpContent
    {
        private readonly HttpContent content;
        private readonly ICompressor compressor;

        public CompressedContent(HttpContent content, ICompressor compressor)
        {
            if (content == null)
            {
                throw new ArgumentNullException(nameof(content));
            }

            if (compressor == null)
            {
                throw new ArgumentNullException(nameof(compressor));
            }

            this.content = content;
            this.compressor = compressor;

            this.AddHeaders();
        }

        protected override bool TryComputeLength(out long length)
        {
            length = -1;
            return false;
        }

        protected override async Task SerializeToStreamAsync(Stream stream, TransportContext context)
        {
            if (stream == null)
            {
                throw new ArgumentNullException(nameof(stream));
            }

            using (this.content)
            {
                var contentStream = await this.content.ReadAsStreamAsync();
                await this.compressor.Compress(contentStream, stream);
            }
        }

        private void AddHeaders()
        {
            foreach (var header in this.content.Headers)
            {
                this.Headers.TryAddWithoutValidation(header.Key, header.Value);
            }

            this.Headers.ContentEncoding.Add(this.compressor.EncodingType);
        }
    }
}