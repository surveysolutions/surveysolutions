using System;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text.RegularExpressions;
using WB.Core.BoundedContexts.QuestionnaireTester.Infrastructure;

namespace WB.Core.Infrastructure.Android.Implementation.Services.FileSystem
{
    internal class FileSystemService : IFileSystemService
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

        public long GetFileSize(string filePath)
        {
            if (!this.IsFileExists(filePath))
                return -1;
            return new FileInfo(filePath).Length;
        }

        public DateTime GetCreationTime(string filePath)
        {
            if (!this.IsFileExists(filePath))
                return DateTime.MinValue;
            return new FileInfo(filePath).CreationTime;
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
            File.WriteAllBytes(pathToFile,content);
        }

        public byte[] ReadAllBytes(string pathToFile)
        {
            return File.ReadAllBytes(pathToFile);
        }

        public string ReadAllText(string fileName)
        {
            return File.ReadAllText(fileName);
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
                    File.Copy(file, CombinePath(destDir, GetFileName(file)));

                foreach (var directory in this.GetDirectoriesInDirectory(sourceDir))
                    CopyFileOrDirectory(directory, destDir);
            }
            else
            {
                this.CopyFile(sourceDir, targetDir);
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

        private void CopyFile(string sourcePath, string backupFolderPath)
        {
            var sourceFileName = GetFileName(sourcePath);
            if (sourceFileName == null)
                return;
            File.Copy(sourcePath, CombinePath(backupFolderPath, sourceFileName), true);
        }
    }
}
