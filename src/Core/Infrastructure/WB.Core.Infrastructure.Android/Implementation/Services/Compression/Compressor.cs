using System.IO;
using System.IO.Compression;

namespace WB.Core.Infrastructure.Android.Implementation.Services.Compression
{
    internal class Compressor
    {
        public byte[] DecompressGZip(byte[] payload)
        {
            using (var msi = new MemoryStream(payload))
            using (var mso = new MemoryStream())
            {
                using (var gs = new GZipStream(msi, CompressionMode.Decompress))
                {
                    gs.CopyTo(mso);
                }
                return mso.ToArray();
            }
        }

        public byte[] DecompressDeflate(byte[] payload)
        {
            using (var msi = new MemoryStream(payload))
            using (var mso = new MemoryStream())
            {
                using (var gs = new DeflateStream(msi, CompressionMode.Decompress))
                {
                    gs.CopyTo(mso);
                }
                return mso.ToArray();
            }
        }

    }
}