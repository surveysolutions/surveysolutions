﻿using System;
using System.IO;
using System.Text.RegularExpressions;

namespace WB.Services.Export.Infrastructure.Implementation
{
    internal class FileSystemAccessor : IFileSystemAccessor
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
        public string CombinePath(params string[] parts) => Combine(parts);
        public bool IsDirectoryExists(string path) => Directory.Exists(path);
        public void DeleteDirectory(string path) => Directory.Delete(path, true);
        public void CreateDirectory(string path) => Directory.CreateDirectory(path);
        public string GetFileName(string path) => Path.GetFileName(path);

        public string GetFileNameWithoutExtension(string filePath) => Path.GetFileNameWithoutExtension(filePath);
        public void DeleteFile(string path) => File.Delete(path);

        public void MoveFile(string fromPath, string toPath) => File.Move(fromPath, toPath);

        public string[] GetFilesInDirectory(string directoryPath)
        {
            return Directory.GetFiles(directoryPath);
        }

        public bool IsFileExists(string filePath) => File.Exists(filePath);
        public byte[] ReadAllBytes(string filePath) => File.ReadAllBytes(filePath);
        public void WriteAllBytes(string filePath, byte[] data)
        {
            File.WriteAllBytes(filePath, data);
        }

        public string GetTempPath(string basePath) => Path.Combine(basePath, ".temp", Guid.NewGuid().ToString());
        public DateTime GetModificationTime(string filePath)
            => this.IsFileExists(filePath) ? new FileInfo(filePath).LastWriteTime : DateTime.MinValue;

        public long GetFileSize(string filePath) => this.IsFileExists(filePath) ? new FileInfo(filePath).Length : -1;
    }
}
