using System;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;
using WB.Core.Infrastructure.FileSystem;
using ZetaLongPaths;
using ZetaLongPaths.Native;
using FileAccess = System.IO.FileAccess;
using FileAttributes = System.IO.FileAttributes;
using FileShare = ZetaLongPaths.Native.FileShare;

namespace WB.Core.Infrastructure.Files.Implementation.FileSystem
{
    internal class FileSystemIOAccessor : IFileSystemAccessor
    {
        public string CombinePath(string path1, string path2)
        {
            return ZlpPathHelper.Combine(path1, path2);
        }

        public string GetFileName(string filePath)
        {
            return ZlpPathHelper.GetFileNameFromFilePath(filePath);
        }

        public string GetFileNameWithoutExtension(string filePath)
        {
            return ZlpPathHelper.GetFileNameWithoutExtension(filePath);
        }

        public long GetFileSize(string filePath)
        {
            if (!this.IsFileExists(filePath))
                return -1;

            try
            {
                return new FileInfo(filePath).Length;
            }
            catch (PathTooLongException)
            {
                return new ZlpFileInfo(filePath).Length;
            }
        }

        public DateTime GetCreationTime(string filePath)
        {
            if (!this.IsFileExists(filePath))
                return DateTime.MinValue;
            try
            {
                return new FileInfo(filePath).CreationTime;
            }
            catch (PathTooLongException)
            {
                return new ZlpFileInfo(filePath).CreationTime;
            }
        }

        public bool IsDirectoryExists(string pathToDirectory)
        {
            return ZlpIOHelper.DirectoryExists(pathToDirectory);
        }

        public void CreateDirectory(string path)
        {
            ZlpIOHelper.CreateDirectory(path);
        }

        public void DeleteDirectory(string path)
        {
            ZlpIOHelper.DeleteDirectory(path, true);
        }

        public bool IsFileExists(string pathToFile)
        {
            return ZlpIOHelper.FileExists(pathToFile);
        }

        public void DeleteFile(string pathToFile)
        {
            ZlpIOHelper.DeleteFile(pathToFile);
        }

        public Stream OpenOrCreateFile(string pathToFile, bool append)
        {
            var stream = new FileStream(
                ZlpIOHelper.CreateFileHandle(pathToFile,
                                             IsFileExists(pathToFile) ? CreationDisposition.OpenExisting : CreationDisposition.New,
                                             ZetaLongPaths.Native.FileAccess.GenericWrite,
                                             FileShare.Write),
                FileAccess.Write);

            if (append && stream.CanSeek)
            {
                stream.Seek(0, SeekOrigin.End);
            }

            return stream;
        }


        public Stream ReadFile(string pathToFile)
        {
            return new FileStream(
                ZlpIOHelper.CreateFileHandle(pathToFile, CreationDisposition.OpenExisting,
                    ZetaLongPaths.Native.FileAccess.GenericRead,
                    FileShare.Read),
                FileAccess.Read);
        }

        public string MakeValidFileName(string name)
        {
            string invalidChars = Regex.Escape(new string(Path.GetInvalidFileNameChars()));
            string invalidReStr = String.Format(@"([{0}]*\.+$)|([{0}]+)", invalidChars);
            return RemoveNonAscii(Regex.Replace(name, invalidReStr, "_"));
        }

        public string[] GetDirectoriesInDirectory(string pathToDirectory)
        {
            return ZlpIOHelper.GetDirectories(pathToDirectory).Select(directoryInfo => directoryInfo.FullName).ToArray();
        }

        public string[] GetFilesInDirectory(string pathToDirectory)
        {
            return ZlpIOHelper.GetFiles(pathToDirectory).Select(fileInfo => fileInfo.FullName).ToArray();
        }

        public string[] GetFilesInDirectory(string pathToDirectory, string pattern)
        {
            return ZlpIOHelper.GetFiles(pathToDirectory, pattern).Select(fileInfo => fileInfo.FullName).ToArray();
        }

        public void WriteAllText(string pathToFile, string content)
        {
            ZlpIOHelper.WriteAllText(pathToFile, content);
        }

        public void WriteAllBytes(string pathToFile, byte[] content)
        {
            ZlpIOHelper.WriteAllBytes(pathToFile, content);
        }

        public byte[] ReadAllBytes(string pathToFile)
        {
            return ZlpIOHelper.ReadAllBytes(pathToFile);
        }

        public string ReadAllText(string pathToFile)
        {
            return ZlpIOHelper.ReadAllText(pathToFile);
        }

        public string ReadAllText(string fileName)
        {
            return ZlpIOHelper.ReadAllText(fileName);
        }

        public void CopyFileOrDirectory(string sourceDir, string targetDir)
        {
            FileAttributes attr = File.GetAttributes(sourceDir);
            if ((attr & FileAttributes.Directory) == FileAttributes.Directory)
            {
                var sourceDirectoryName = GetFileName(sourceDir);
                if (sourceDirectoryName == null)
                    return;
                var destDir = this.CombinePath(targetDir, sourceDirectoryName);
                CreateDirectory(destDir);

                foreach (var file in GetFilesInDirectory(sourceDir))
                    ZlpIOHelper.CopyFile(file, CombinePath(destDir, GetFileName(file)), true);

                foreach (var directory in this.GetDirectoriesInDirectory(sourceDir))
                    CopyFileOrDirectory(directory, CombinePath(destDir, sourceDirectoryName));
            }
            else
            {
                this.CopyFile(sourceDir, targetDir);
            }
        }

        private string RemoveNonAscii(string s)
        {
            return Regex.Replace(s, @"[^\u0000-\u007F]", string.Empty);
        }

        private void CopyFile(string sourcePath, string backupFolderPath)
        {
            var sourceFileName = GetFileName(sourcePath);
            if (sourceFileName == null)
                return;
            ZlpIOHelper.CopyFile(sourcePath, CombinePath(backupFolderPath, sourceFileName), true);
        }
    }
}
