using System.Collections.Generic;

namespace SynchronizationMessages.Synchronization
{
    using System;
    using System.IO;
    using System.IO.Compression;
    using System.Text;

    public static class PackageHelper
    {
        public static byte[] Compress(string text)
        {
            byte[] buffer = Encoding.UTF8.GetBytes(text);
            using (var ms = new MemoryStream())
            {
                using (GZipStream zip = new GZipStream(ms, CompressionMode.Compress, true))
                {
                    zip.Write(buffer, 0, buffer.Length);
                }

                ms.Position = 0;

                byte[] compressed = new byte[ms.Length];
                ms.Read(compressed, 0, compressed.Length);
                return compressed;
            }
        }

        public static string Decompress(Stream stream)
        {
            stream.Position = 0;
            var result = new List<byte>();
            using (GZipStream zip = new GZipStream(stream, CompressionMode.Decompress))
            {
                byte[] buffer = new byte[1024];
                while (zip.Read(buffer, 0, buffer.Length) > 0)
                {
                    result.AddRange(buffer);
                    buffer = new byte[1024];
                }
            }
            return Encoding.UTF8.GetString(result.ToArray());
        }
    }
}
