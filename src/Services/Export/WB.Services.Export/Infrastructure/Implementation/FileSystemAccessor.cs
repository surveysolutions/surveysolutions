using System;
using System.IO;
using System.Text.RegularExpressions;

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

        public string MakeValidFileName(string name)
        {
            if (string.IsNullOrEmpty(name))
                return string.Empty;

            var invalidChars = Regex.Escape(new string(Path.GetInvalidFileNameChars()));
            var invalidReStr = string.Format(@"([{0}]*\.+$)|([{0}]+)", invalidChars);
            var fileNameWithReplaceInvalidChars = Regex.Replace(name, invalidReStr, "_");
            return fileNameWithReplaceInvalidChars.Substring(0, Math.Min(fileNameWithReplaceInvalidChars.Length, 128));
        }

        public string Combine(params string[] parts) => Path.Combine(parts);
    }
}
