using System;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

using WB.Core.Infrastructure.FileSystem;
using FileAccess = System.IO.FileAccess;

namespace WB.UI.Shared.Enumerator.Services.Internals.FileSystem
{
    internal class FileSystemService : FileSystemAccessorBase, IFileSystemAccessor
    {
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

        public string GetDirectoryName(string path)
        {
            return new DirectoryInfo(path).Name;
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

        public string[] GetFilesInDirectory(string pathToDirectory, string pattern, bool searchInSubdirectories = false)
        {
            return Directory.GetFiles(pathToDirectory, pattern, searchInSubdirectories ? SearchOption.AllDirectories : SearchOption.TopDirectoryOnly).ToArray();
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
                    if (!Path.GetFullPath(directory).Equals(Path.GetFullPath(targetGlobalDir)))
                        this.CopyFileOrDirectory(directory, destDir, overrideAll, fileExtentionsFilter);
                }

                var files = this.GetFilesInDirectory(sourceDir, false);
                foreach (var file in files)
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

        private void CopyFile(string sourcePath, string backupFolderPath, bool overrideAll, string[] fileExtensionsFilter)
        {
            CopyFileAsync(sourcePath, backupFolderPath, overrideAll, fileExtensionsFilter).Wait();
        }

        private async Task CopyFileAsync(string sourcePath, string backupFolderPath, bool overrideAll, string[] fileExtensionsFilter)
        {
            var sourceFileName = this.GetFileName(sourcePath);
            if (sourceFileName == null)
                return;

            if (fileExtensionsFilter != null)
            {
                if (!fileExtensionsFilter.Contains(Path.GetExtension(sourcePath), StringComparer.InvariantCultureIgnoreCase))
                    return;
            }

            if (!overrideAll && Directory.EnumerateFiles(backupFolderPath).Any(f => Path.GetFileName(f) == sourceFileName))
                return;
            
            await using var destFile = File.Create(Path.Combine(backupFolderPath, Path.GetFileName(sourcePath)));
            await using var outFile = File.OpenRead(sourcePath);
            await outFile.CopyToAsync(destFile);
        }

        public string ChangeExtension(string path1, string newExtension)
        {
            return Path.ChangeExtension(path1, newExtension);
        }

        public void MoveFile(string pathToFile, string newPathToFile)
        {
            //File.Copy throws exception
            //https://github.com/xamarin/xamarin-android/issues/3426
            //workaround

            File.Delete(newPathToFile);

            using (FileStream sourceStream = new FileStream(pathToFile, FileMode.Open))
            {
                byte[] buffer = new byte[1024 * 1024];
                using (FileStream destStream = new FileStream(newPathToFile, FileMode.Create))
                {
                    int i;
                    while ((i = sourceStream.Read(buffer, 0, buffer.Length)) > 0)
                    {
                        destStream.Write(buffer, 0, i);
                    }
                }
            }

            File.Delete(pathToFile);
        }

        public void MoveDirectory(string pathToDir, string newPathToDir)
        {
            Directory.Move(pathToDir, newPathToDir);
        }
    }
}
