using System.IO;
using System.IO.Compression;
using System.Threading;
using System.Threading.Tasks;
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

        public void CreateEntry(string path, Stream content)
        {
            var entry = new ZipEntry(path);

            zipStream.PutNextEntry(entry);

            if (content.Length != 0)
            {
                content.CopyTo(zipStream);
            }

            zipStream.CloseEntry();
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
        
        public async Task CreateEntryAsync(string path, byte[] content, CancellationToken token = default)
        {
            var entry = new ZipEntry(path);
            
            zipStream.PutNextEntry(entry);

            if (content.Length != 0)
            {
                await zipStream.WriteAsync(content, 0, content.Length, token);
            }

            zipStream.CloseEntry();
        }

        public async Task CreateEntryAsync(string path, Stream content, CancellationToken token = default)
        {
            var entry = new ZipEntry(path);

            zipStream.PutNextEntry(entry);

            if (content.Length != 0)
            {
                await content.CopyToAsync(zipStream, 81920, token);
            }

            zipStream.CloseEntry();
        }
    }
}
