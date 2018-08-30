using System;
using System.IO;

namespace WB.Core.Infrastructure.FileSystem
{
    public interface IFileSystemAccessor
    {
        string CombinePath(string path1, string path2);
        string CombinePath(params string[] pathes);
        string GetFileName(string filePath);
        string GetFileNameWithoutExtension(string filePath);
        string GetFileExtension(string filePath);
        long GetFileSize(string filePath);
        long GetDirectorySize(string path);
        DateTime GetCreationTime(string filePath);
        DateTime GetModificationTime(string filePath);
        bool IsDirectoryExists(string pathToDirectory);
        void CreateDirectory(string path);
        void DeleteDirectory(string path);
        string GetDirectory(string path);

        bool IsFileExists(string pathToFile);
        Stream OpenOrCreateFile(string pathToFile, bool append);
        Stream ReadFile(string pathToFile);
        void DeleteFile(string pathToFile);

        string MakeStataCompatibleFileName(string name);
        string MakeValidFileName(string name);

        string[] GetDirectoriesInDirectory(string pathToDirectory);
        string[] GetFilesInDirectory(string pathToDirectory, bool searchInSubdirectories = false);
        string[] GetFilesInDirectory(string pathToDirectory, string pattern);

        void WriteAllText(string pathToFile, string content);
        void WriteAllBytes(string pathToFile, byte[] content);
        byte[] ReadAllBytes(string pathToFile);
        string ReadAllText(string pathToFile);

        void CopyFileOrDirectory(string sourceDir, string targetDir, bool overrideAll = false, string[] fileExtentionsFilter = null);

        void MarkFileAsReadonly(string pathToFile);

        bool IsWritePermissionExists(string path);

        string ChangeExtension(string path1, string newExtension);

        void MoveFile(string pathToFile, string newPathToFile);

    }
}
