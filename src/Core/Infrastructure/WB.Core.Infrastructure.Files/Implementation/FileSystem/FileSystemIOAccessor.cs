using System;
using System.CodeDom;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Security.AccessControl;
using System.Security.Principal;
using System.Text;
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
            if (string.IsNullOrEmpty(name))
                return string.Empty;
            string invalidChars = Regex.Escape(new string(Path.GetInvalidFileNameChars()));
            string invalidReStr = String.Format(@"([{0}]*\.+$)|([{0}]+)", invalidChars);
            var result = RemoveNonAscii(Regex.Replace(name, invalidReStr, "_")).Trim();
            if (result.Length < 128)
                return result;
            return result.Substring(0, 128);
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

        public void MarkFileAsReadonly(string pathToFile)
        {
            ZlpIOHelper.SetFileAttributes(pathToFile, ZetaLongPaths.Native.FileAttributes.Readonly);
        }

        public Assembly LoadAssembly(string assemblyFile)
        {
            //please don't use LoadFile or Load here, but use LoadFrom
            //dependent assemblies could not be resolved
            return Assembly.LoadFrom(assemblyFile);
        }

        public bool IsWritePermissionExists(string path)
        {
            if (string.IsNullOrEmpty(path) || !Directory.Exists(path))
                return false;

            try
            {
                DirectorySecurity security = Directory.GetAccessControl(path);
                var authorizationRuleCollection = security.GetAccessRules(true, true, typeof(SecurityIdentifier));

                var windowsIdentity = WindowsIdentity.GetCurrent();
                var identityReferences = new List<IdentityReference> { windowsIdentity.User };
                if (windowsIdentity.Groups != null)
                    identityReferences.AddRange(windowsIdentity.Groups);

                var isAllowWriteForUser = IsAllowWriteForIdentityReferance(authorizationRuleCollection, identityReferences);
                return isAllowWriteForUser;
            }
            catch (UnauthorizedAccessException)
            {
                return false;
            }
        }

        private bool IsAllowWriteForIdentityReferance(
            AuthorizationRuleCollection authorizationRuleCollection, List<IdentityReference> identityReferences)
        {
            var writeAllow = false;
            var writeDeny = false;

            foreach (AuthorizationRule authorizationRule in authorizationRuleCollection)
            {
                var isUserPermission = identityReferences.Any(ir => ir.Equals(authorizationRule.IdentityReference));
                if (isUserPermission)
                {
                    FileSystemAccessRule rule = ((FileSystemAccessRule)authorizationRule);

                    if ((rule.FileSystemRights & FileSystemRights.WriteData) == FileSystemRights.WriteData)
                    {
                        if (rule.AccessControlType == AccessControlType.Allow)
                            writeAllow = true;
                        else if (rule.AccessControlType == AccessControlType.Deny)
                            writeDeny = true;
                    }
                }
            }

            return writeAllow && !writeDeny;
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
