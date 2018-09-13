using System.IO;

namespace WB.Services.Export.Infrastructure.Implementation
{
    public class FileSystemAccessor : IFileSystemAccessor
    {
        public Stream OpenOrCreateFile(string pathToFile, bool append)
        {
            var stream = File.OpenWrite(pathToFile);

            if (append && stream.CanSeek)
                stream.Seek(0, SeekOrigin.End);

            return stream;
        }

        public void WriteAllText(string pathToFile, string content) => File.WriteAllText(pathToFile, content);
    }
}
