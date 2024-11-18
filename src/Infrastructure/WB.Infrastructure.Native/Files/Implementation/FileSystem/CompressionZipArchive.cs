using System.IO;
using System.IO.Compression;
using WB.Core.Infrastructure.FileSystem;

namespace WB.Infrastructure.Native.Files.Implementation.FileSystem
{
    public class CompressionZipArchive : IZipArchive
    {
        private readonly ZipArchive zipArchive;

        public CompressionZipArchive(Stream outputStream)
        {
            this.zipArchive = new ZipArchive(outputStream, ZipArchiveMode.Create, true);
        }

        public void Dispose()
        {
            zipArchive.Dispose();
        }

        public void CreateEntry(string path, byte[] content)
        {
            var entry = zipArchive.CreateEntry(path, CompressionLevel.Optimal);
            using var entryStream = entry.Open();
            entryStream.Write(content, 0, content.Length);
        }
    }
}
