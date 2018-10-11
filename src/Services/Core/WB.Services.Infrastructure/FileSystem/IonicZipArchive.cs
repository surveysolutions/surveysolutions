using System.IO;
using System.IO.Compression;
using ICSharpCode.SharpZipLib.Zip;
using ICSharpCode.SharpZipLib.Zip.Compression;

namespace WB.Services.Infrastructure.FileSystem
{
    public class IonicZipArchive : IZipArchive
    {
        private readonly bool leaveOpen;
        private readonly ZipOutputStream zipStream;

        public IonicZipArchive(Stream outputStream, string password, CompressionLevel compressionLevel = CompressionLevel.Fastest, bool leaveOpen = false)
        {
            this.leaveOpen = leaveOpen;
            this.zipStream = new ZipOutputStream(outputStream);
            zipStream.UseZip64 = UseZip64.Dynamic;
            
            switch (compressionLevel)
            {
                case CompressionLevel.Fastest:
                    zipStream.SetLevel(Deflater.BEST_SPEED);
                    break;
                case CompressionLevel.NoCompression:
                    zipStream.SetLevel(Deflater.NO_COMPRESSION);
                    break;
                case CompressionLevel.Optimal:
                    zipStream.SetLevel(Deflater.BEST_COMPRESSION);
                    break;
            }
            

            if (!string.IsNullOrWhiteSpace(password))
            {
                zipStream.Password = password;
            }
        }

        public void Dispose()
        {
            if (leaveOpen) return;

            zipStream.Dispose();
        }

        public void CreateEntry(string path, byte[] content)
        {
            var entry = new ZipEntry(path);
            
            zipStream.PutNextEntry(entry);

            if (content.Length != 0)
            {
                zipStream.Write(content, 0, content.Length);
            }

            zipStream.CloseEntry();
        }
    }
}
