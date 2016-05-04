using System.IO;
using System.Threading.Tasks;
using WB.Core.GenericSubdomains.Portable.Services;

namespace WB.Core.GenericSubdomains.Portable.Implementation.Services
{
    public abstract class Compressor : ICompressor
    {
        public abstract string EncodingType { get; }
        public abstract Stream CreateCompressionStream(Stream output);
        public abstract Stream CreateDecompressionStream(Stream input);

        public virtual Task Compress(Stream source, Stream destination)
        {
            var compressed = this.CreateCompressionStream(destination);

            return this.Pump(source, compressed)
                .ContinueWith(task => compressed.Dispose());
        }

        public virtual Task Decompress(Stream source, Stream destination)
        {
            var decompressed = this.CreateDecompressionStream(source);

            return this.Pump(decompressed, destination)
                .ContinueWith(task => decompressed.Dispose());
        }

        protected virtual Task Pump(Stream input, Stream output)
        {
            return input.CopyToAsync(output);
        }
    }
}