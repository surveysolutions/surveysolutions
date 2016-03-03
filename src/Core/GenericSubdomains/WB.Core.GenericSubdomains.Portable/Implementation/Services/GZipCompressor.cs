using System.IO;
using System.IO.Compression;

namespace WB.Core.GenericSubdomains.Portable.Implementation.Services
{
    public class GZipCompressor : Compressor
    {
        private const string GZipEncoding = "gzip";
        public override string EncodingType => GZipEncoding;

        public override Stream CreateCompressionStream(Stream output)
        {
            return new GZipStream(output, CompressionMode.Compress, leaveOpen: true);
        }

        public override Stream CreateDecompressionStream(Stream input)
        {
            return new GZipStream(input, CompressionMode.Decompress, leaveOpen: true);
        }
    }
}