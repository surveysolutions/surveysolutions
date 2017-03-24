using System;
using System.Collections.Generic;
using System.IO;
using ICSharpCode.SharpZipLib.VirtualFileSystem;
using VFSFileAttributes = ICSharpCode.SharpZipLib.VirtualFileSystem.FileAttributes;

namespace WB.Infrastructure.Shared.Enumerator.Internals.FileSystem
{
    internal class AndroidFileSystem : IVirtualFileSystem
    {
        public IEnumerable<string> GetFiles(string directory)
        {
            return Directory.GetFiles(directory);
        }

        public IEnumerable<string> GetDirectories(string directory)
        {
            return Directory.GetDirectories(directory);
        }

        public string GetFullPath(string path)
        {
            return Path.GetFullPath(path);
        }

        public IDirectoryInfo GetDirectoryInfo(string directoryName)
        {
            return new AndroidDirectoryInfo(new DirectoryInfo(directoryName));
        }

        public IFileInfo GetFileInfo(string filename)
        {
            return new AndroidFileInfo(new FileInfo(filename));
        }

        public void SetLastWriteTime(string name, DateTime dateTime)
        {
            File.SetLastWriteTime(name, dateTime);
        }

        public void SetAttributes(string name, VFSFileAttributes attributes) {}

        public void CreateDirectory(string directory)
        {
            Directory.CreateDirectory(directory);
        }

        public string GetTempFileName()
        {
            return Path.GetTempFileName();
        }

        public void CopyFile(string fromFileName, string toFileName, bool overwrite)
        {
            File.Copy(fromFileName, toFileName, overwrite);
        }

        public void MoveFile(string fromFileName, string toFileName)
        {
            File.Move(fromFileName, toFileName);
        }

        public void DeleteFile(string fileName)
        {
            File.Delete(fileName);
        }

        public VfsStream CreateFile(string filename)
        {
            return new VfsProxyStream(File.Create(filename), filename);
        }

        public VfsStream OpenReadFile(string filename)
        {
            return new VfsProxyStream(File.OpenRead(filename), filename);
        }

        public VfsStream OpenWriteFile(string filename)
        {
            return new VfsProxyStream(File.OpenWrite(filename), filename);
        }

        public string CurrentDirectory
        {
            get { return Environment.CurrentDirectory; }
        }

        public char DirectorySeparatorChar
        {
            get { return Path.DirectorySeparatorChar; }
        }
    }
}