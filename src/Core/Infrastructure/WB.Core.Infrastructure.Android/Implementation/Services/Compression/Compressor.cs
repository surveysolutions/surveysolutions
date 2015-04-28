using System.IO;
using System.IO.Compression;
using System.Threading.Tasks;
using WB.Core.BoundedContexts.QuestionnaireTester.Infrastructure;

namespace WB.Core.Infrastructure.Android.Implementation.Services.Compression
{
    public class Compressor : ICompressor
    {
        public Task<byte[]> DecompressGZipAsync(byte[] payload)
        {
            return Task.Run(() =>
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
            });
        }

        public Task<byte[]> DecompressDeflateAsync(byte[] payload)
        {
            return Task.Run(() =>
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
            });
        }

    }
}