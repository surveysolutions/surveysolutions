using System;
using System.IO;
using System.Reflection;
using System.Text;

namespace WB.Core.Infrastructure.FileSystem
{
    public interface IFileSystemAccessor
    {
        string CombinePath(string path1, string path2);
        string GetFileName(string filePath);
        string GetFileNameWithoutExtension(string filePath);
        long GetFileSize(string filePath);
        DateTime GetCreationTime(string filePath);
        bool IsDirectoryExists(string pathToDirectory);
        void CreateDirectory(string path);
        void DeleteDirectory(string path);

        bool IsFileExists(string pathToFile);
        Stream OpenOrCreateFile(string pathToFile, bool append);
        Stream ReadFile(string pathToFile);
        void DeleteFile(string pathToFile);

        string MakeValidFileName(string name);

        string[] GetDirectoriesInDirectory(string pathToDirectory);
        string[] GetFilesInDirectory(string pathToDirectory);
        string[] GetFilesInDirectory(string pathToDirectory, string pattern);

        void WriteAllText(string pathToFile, string content);
        void WriteAllBytes(string pathToFile, byte[] content);
        byte[] ReadAllBytes(string pathToFile);
        string ReadAllText(string pathToFile);

        void CopyFileOrDirectory(string sourceDir, string targetDir);

        void MarkFileAsReadonly(string pathToFile);

        Assembly LoadAssembly(string assemblyFile);

        bool IsWritePermissionExists(string path);

        string ChangeExtension(string path1, string newExtension);
    }
}
