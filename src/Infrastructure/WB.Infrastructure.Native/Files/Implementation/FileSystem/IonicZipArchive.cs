using System.IO;
using System.Text;
using Ionic.Zip;
using WB.Core.Infrastructure.FileSystem;

namespace WB.Infrastructure.Native.Files.Implementation.FileSystem
{
    public class IonicZipArchive : IZipArchive
    {
        private readonly Stream outputStream;
        private readonly ZipFile zipFile;

        public IonicZipArchive(Stream outputStream, string password)
        {
            this.outputStream = outputStream;
            zipFile = new ZipFile
            {
                ParallelDeflateThreshold = -1,
                AlternateEncoding = Encoding.UTF8,
                AlternateEncodingUsage = ZipOption.Always,
                UseZip64WhenSaving = Zip64Option.AsNecessary
            };

            if (!string.IsNullOrWhiteSpace(password))
            {
                zipFile.Password = password;
            }
        }

        public void Dispose()
        {
            zipFile.Save(outputStream);
        }

        public void CreateEntry(string path, byte[] content)
        {
            zipFile.AddEntry(path, content);
        }
    }
}