using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using WB.Core.Infrastructure.FileSystem;

namespace WB.Core.Infrastructure.Files.Implementation.FileSystem
{
    internal class FileSystemIoAccessor : IFileSystemAccessor
    {
        public string CombinePath(params string[] pathParts)
        {
            return Path.Combine(pathParts);
        }

        public string GetFileName(string filePath)
        {
            return Path.GetFileName(filePath);
        }

        public long GetFileSize(string filePath)
        {
            if (!this.IsFileExists(filePath))
                return -1;
            return new FileInfo(filePath).Length;
        }

        public bool IsDirectoryExists(string pathToDirectory)
        {
            return Directory.Exists(pathToDirectory);
        }

        public void CreateDirectory(string path)
        {
            Directory.CreateDirectory(path);
        }

        public void DeleteDirectory(string path)
        {
            Directory.Delete(path, true);
        }

        public bool IsFileExists(string pathToFile)
        {
            return File.Exists(pathToFile);
        }

        public void DeleteFile(string pathToFile)
        {
            File.Delete(pathToFile);
        }

        public string MakeValidFileName(string name)
        {
            string invalidChars = Regex.Escape(new string(Path.GetInvalidFileNameChars()));
            string invalidReStr = String.Format(@"([{0}]*\.+$)|([{0}]+)", invalidChars);
            return RemoveNonAscii(Regex.Replace(name, invalidReStr, "_"));
        }

        public string[] GetDirectoriesInDirectory(string pathToDirectory)
        {
            return Directory.GetDirectories(pathToDirectory).ToArray();
        }

        public string[] GetFilesInDirectory(string pathToDirectory)
        {
            return Directory.GetFiles(pathToDirectory).ToArray();
        }

        public void WriteAllText(string pathToFile, string content)
        {
            File.WriteAllText(pathToFile, content);
        }

        public byte[] ReadAllBytes(string pathToFile)
        {
            return File.ReadAllBytes(pathToFile);
        }

        private string RemoveNonAscii(string s)
        {
            return Regex.Replace(s, @"[^\u0000-\u007F]", string.Empty);
        }
    }
}
