using System.IO;
using System.Threading.Tasks;
using WB.Core.GenericSubdomains.Portable.Services;

namespace WB.Core.GenericSubdomains.Portable.Implementation.Compression
{
    /// <summary>
    /// Backported from https://github.com/azzlack/Microsoft.AspNet.WebApi.MessageHandlers.Compression
    /// </summary>
    public abstract class Compressor : ICompressor
    {
        public abstract string EncodingType { get; }
        public abstract Stream CreateCompressionStream(Stream output);
        public abstract Stream CreateDecompressionStream(Stream input);

        public virtual async Task<long> Compress(Stream source, Stream destination)
        {
            using (var mem = new MemoryStream())
            {
                using (var gzip = this.CreateCompressionStream(mem))
                {
                    await source.CopyToAsync(gzip);
                }

                mem.Position = 0;

                var compressed = new byte[mem.Length];
                await mem.ReadAsync(compressed, 0, compressed.Length);

                var outStream = new MemoryStream(compressed);
                await outStream.CopyToAsync(destination);

                return mem.Length;
            }
        }

        public virtual async Task Decompress(Stream source, Stream destination)
        {
            var decompressed = this.CreateDecompressionStream(source);

            await this.Pump(decompressed, destination);

            decompressed.Dispose();
        }

        protected virtual Task Pump(Stream input, Stream output)
        {
            return input.CopyToAsync(output);
        }
    }
}