using System;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text.RegularExpressions;
using WB.Core.Infrastructure.FileSystem;

namespace WB.Infrastructure.Shared.Enumerator.Internals.FileSystem
{
    internal class FileSystemService : IFileSystemAccessor
    {
        public string CombinePath(string path1, string path2)
        {
            return Path.Combine(path1, path2);
        }

        public string GetFileName(string filePath)
        {
            return Path.GetFileName(filePath);
        }

        public string GetFileNameWithoutExtension(string filePath)
        {
            return Path.GetFileNameWithoutExtension(filePath);
        }

        public string GetFileExtension(string filePath)
        {
            return Path.GetExtension(filePath);
        }

        public long GetFileSize(string filePath)
        {
            if (!this.IsFileExists(filePath))
                return -1;
            return new FileInfo(filePath).Length;
        }

        public long GetDirectorySize(string path)
        {
            long size = 0;
            // Add file sizes.
            var filesInDirectory = GetFilesInDirectory(path);
            foreach (var file in filesInDirectory)
            {
                size += GetFileSize(file);
            }
            // Add subdirectory sizes.
            var nestedDirectories = GetDirectoriesInDirectory(path);
            foreach (var nestedDirectory in nestedDirectories)
            {
                size += GetDirectorySize(nestedDirectory);
            }
            return (size);
        }

        public DateTime GetCreationTime(string filePath)
        {
            if (!this.IsFileExists(filePath))
                return DateTime.MinValue;
            return new FileInfo(filePath).CreationTime;
        }

        public DateTime GetModificationTime(string filePath)
        {
            if (!this.IsFileExists(filePath))
                return DateTime.MinValue;
            return new FileInfo(filePath).LastWriteTime;
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

        public Stream OpenOrCreateFile(string pathToFile, bool append)
        {
            return new FileStream(pathToFile, append ? FileMode.Append : FileMode.Create, FileAccess.Write);
        }

        public Stream ReadFile(string pathToFile)
        {
            return new FileStream(pathToFile, FileMode.Open, FileAccess.Read);
        }

        public string MakeStataCompatibleFileName(string name)
        {
            return this.RemoveNonAscii(MakeValidFileName(name));
        }

        public string MakeValidFileName(string name)
        {
            string invalidChars = Regex.Escape(new string(Path.GetInvalidFileNameChars()));
            string invalidReStr = String.Format(@"([{0}]*\.+$)|([{0}]+)", invalidChars);
            return Regex.Replace(name, invalidReStr, "_");
        }

        public string[] GetDirectoriesInDirectory(string pathToDirectory)
        {
            return Directory.GetDirectories(pathToDirectory).ToArray();
        }

        public string[] GetFilesInDirectory(string pathToDirectory, bool searchInSubdirectories = false)
            => Directory.GetFiles(pathToDirectory, "*.*", searchInSubdirectories ? SearchOption.AllDirectories : SearchOption.TopDirectoryOnly).ToArray();

        public string[] GetFilesInDirectory(string pathToDirectory, string pattern)
        {
            return Directory.GetFiles(pathToDirectory, pattern).ToArray();
        }

        public void WriteAllText(string pathToFile, string content)
        {
            File.WriteAllText(pathToFile, content);
        }

        public void WriteAllBytes(string pathToFile, byte[] content)
        {
            File.WriteAllBytes(pathToFile, content);
        }

        public byte[] ReadAllBytes(string pathToFile)
        {
            return File.ReadAllBytes(pathToFile);
        }

        public string ReadAllText(string fileName)
        {
            return File.ReadAllText(fileName);
        }

        public void CopyFileOrDirectory(string sourceDir, string targetDir, bool overrideAll = false)
        {
            FileAttributes attr = File.GetAttributes(sourceDir);
            if ((attr & FileAttributes.Directory) == FileAttributes.Directory)
            {
                var sourceDirectoryName = this.GetFileName(sourceDir);
                if (sourceDirectoryName == null)
                    return;
                var destDir = this.CombinePath(targetDir, sourceDirectoryName);
                this.CreateDirectory(destDir);

                foreach (var file in this.GetFilesInDirectory(sourceDir))
                    File.Copy(file, this.CombinePath(destDir, this.GetFileName(file)), overrideAll);

                foreach (var directory in this.GetDirectoriesInDirectory(sourceDir))
                    this.CopyFileOrDirectory(directory, destDir, overrideAll);
            }
            else
            {
                this.CopyFile(sourceDir, targetDir, overrideAll);
            }
        }

        public void MarkFileAsReadonly(string pathToFile)
        {
            File.SetAttributes(pathToFile, FileAttributes.ReadOnly);
        }

        public Assembly LoadAssembly(string assemblyFile)
        {
            //please don't use LoadFile or Load here, but use LoadFrom
            //dependent assemblies could not be resolved
            return Assembly.LoadFrom(assemblyFile);
        }

        public bool IsWritePermissionExists(string path)
        {
            throw new NotImplementedException();
        }

        private string RemoveNonAscii(string s)
        {
            return Regex.Replace(s, @"[^\u0000-\u007F]", string.Empty);
        }

        private void CopyFile(string sourcePath, string backupFolderPath, bool overrideAll = false)
        {
            var sourceFileName = this.GetFileName(sourcePath);
            if (sourceFileName == null)
                return;
            File.Copy(sourcePath, this.CombinePath(backupFolderPath, sourceFileName), overrideAll);
        }

        public string ChangeExtension(string path1, string newExtension)
        {
            return Path.ChangeExtension(path1, newExtension);
        }
    }
}
