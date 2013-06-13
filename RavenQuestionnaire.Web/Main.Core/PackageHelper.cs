using System;
using System.Collections.Generic;
using System.IO;
using System.IO.Compression;
using System.Text;

namespace Main.Core
{
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

        public static string CompressString(string s)
        {
            var bytes = Encoding.Unicode.GetBytes(s);
            using (var msi = new MemoryStream(bytes))
            using (var mso = new MemoryStream())
            {
                using (var gs = new GZipStream(mso, CompressionMode.Compress))
                {
                    msi.CopyTo(gs);
                }
                return Convert.ToBase64String(mso.ToArray());
            }
        }

        public static string DecompressString(string s)
        {
            var bytes = Convert.FromBase64String(s);
            using (var msi = new MemoryStream(bytes))
            using (var mso = new MemoryStream())
            {
                using (var gs = new GZipStream(msi, CompressionMode.Decompress))
                {
                    gs.CopyTo(mso);
                }
                return Encoding.Unicode.GetString(mso.ToArray());
            }
        }
    }
}
