using System.IO;
using System.IO.Compression;

namespace WB.Core.GenericSubdomains.Portable.Implementation.Services
{
    public class DeflateCompressor : Compressor
    {
        private const string DeflateEncoding = "deflate";

        public override string EncodingType => DeflateEncoding;

        public override Stream CreateCompressionStream(Stream output)
        {
            return new DeflateStream(output, CompressionMode.Compress, leaveOpen: true);
        }

        public override Stream CreateDecompressionStream(Stream input)
        {
            return new DeflateStream(input, CompressionMode.Decompress, leaveOpen: true);
        }
    }
}