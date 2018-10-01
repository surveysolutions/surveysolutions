using System;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;
using WB.Core.Infrastructure.FileSystem;

namespace WB.UI.Shared.Enumerator.Services.Internals.FileSystem
{
    internal class FileSystemService : IFileSystemAccessor
    {
        public string CombinePath(string path1, string path2)
        {
            return Path.Combine(path1, path2);
        }

        public string CombinePath(params string[] pathes) => Path.Combine(pathes);

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

        public string GetDirectory(string path)
        {
            return Path.GetDirectoryName(path);
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

        public byte[] ReadAllBytes(string pathToFile, long? start = null, long? length = null)
        {
            if (start != null)
            {
                using (var reader = File.OpenRead(pathToFile))
                {
                    return reader.ReadExactly(start.Value, length);
                }
            }
            else return File.ReadAllBytes(pathToFile);
        }

        public string ReadAllText(string fileName)
        {
            return File.ReadAllText(fileName);
        }

        public void CopyFileOrDirectory(string sourceDir, string targetDir, bool overrideAll = false, string[] fileExtentionsFilter = null)
        {
            CopyFileOrDirectoryInt(sourceDir, targetDir, targetDir, overrideAll, fileExtentionsFilter);
        }

        private void CopyFileOrDirectoryInt(string sourceDir, string targetDir, string targetGlobalDir, bool overrideAll, string[] fileExtentionsFilter)
        {

            FileAttributes attr = File.GetAttributes(sourceDir);
            if ((attr & FileAttributes.Directory) == FileAttributes.Directory)
            {
                var sourceDirectoryName = this.GetFileName(sourceDir);
                if (sourceDirectoryName == null)
                    return;

                var dirList = this.GetDirectoriesInDirectory(sourceDir);

                var destDir = this.CombinePath(targetDir, sourceDirectoryName);
                if (!Directory.Exists(destDir))
                    this.CreateDirectory(destDir);

                foreach (var directory in dirList)
                {
                    if(!Path.GetFullPath(directory).Equals(Path.GetFullPath(targetGlobalDir)))
                        this.CopyFileOrDirectory(directory, destDir, overrideAll, fileExtentionsFilter);
                }

                foreach (var file in this.GetFilesInDirectory(sourceDir, false))
                    this.CopyFile(file, destDir, overrideAll, fileExtentionsFilter);
            }
            else
            {
                this.CopyFile(sourceDir, targetDir, overrideAll, fileExtentionsFilter);
            }
        }

        public void MarkFileAsReadonly(string pathToFile)
        {
            File.SetAttributes(pathToFile, FileAttributes.ReadOnly);
        }

        public bool IsWritePermissionExists(string path)
        {
            throw new NotImplementedException();
        }

        private string RemoveNonAscii(string s)
        {
            return Regex.Replace(s, @"[^\u0000-\u007F]", string.Empty);
        }

        private void CopyFile(string sourcePath, string backupFolderPath, bool overrideAll, string[] fileExtentionsFilter)
        {
            var sourceFileName = this.GetFileName(sourcePath);
            if (sourceFileName == null)
                return;

            if (fileExtentionsFilter != null)
            {
                if(!fileExtentionsFilter.Contains(Path.GetExtension(sourcePath), StringComparer.InvariantCultureIgnoreCase))
                    return;                
            }

            File.Copy(sourcePath, this.CombinePath(backupFolderPath, sourceFileName), overrideAll);
        }

        public string ChangeExtension(string path1, string newExtension)
        {
            return Path.ChangeExtension(path1, newExtension);
        }

        public void MoveFile(string pathToFile, string newPathToFile)
        {
            if (File.Exists(newPathToFile))
                File.Delete(newPathToFile);
            File.Move(pathToFile, newPathToFile);
        }
    }
}
