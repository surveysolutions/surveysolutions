using System.IO;
using System.Text;
using Ionic.Zip;
using Ionic.Zlib;
using WB.Core.Infrastructure.FileSystem;

namespace WB.Infrastructure.Native.Files.Implementation.FileSystem
{
    public class IonicZipArchive : IZipArchive
    {
        private readonly ZipOutputStream zipStream;

        public IonicZipArchive(Stream outputStream, string password, CompressionLevel compressionLevel = CompressionLevel.BestSpeed, bool leaveOpen = false)
        {
            this.zipStream = new ZipOutputStream(outputStream, leaveOpen);
            zipStream.EnableZip64 = Zip64Option.AsNecessary;
            zipStream.CompressionLevel = compressionLevel;
            
            if (!string.IsNullOrWhiteSpace(password))
            {
                zipStream.Password = password;
            }
        }

        public void Dispose()
        {
            zipStream.Dispose();
        }

        public void CreateEntry(string path, byte[] content)
        {
            zipStream.PutNextEntry(path);

            if (content.Length != 0)
            {
                zipStream.Write(content, 0, content.Length);
            }
        }
    }
}
