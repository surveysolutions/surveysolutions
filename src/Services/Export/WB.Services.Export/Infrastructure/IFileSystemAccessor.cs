using System;
using System.IO;
using System.Threading.Tasks;

namespace WB.Services.Export.Infrastructure
{
    public interface IFileSystemAccessor
    {
        Stream OpenOrCreateFile(string filePath, bool append);
        void WriteAllText(string contentFilePath, string toString);
        Task WriteAllTextAsync(string contentFilePath, string toString);
        string MakeValidFileName(string questionnaireTitle);
        string Combine(params string[] parts);
        string CombinePath(params string[] parts);
        bool IsDirectoryExists(string path);
        void DeleteDirectory(string path);
        void CreateDirectory(string path);
        string GetFileName(string path);
        string GetFileNameWithoutExtension(string filePath);
        void DeleteFile(string path);
        void MoveFile(string fromPath, string toPath);
        string[] GetFilesInDirectory(string directoryPath);
        string[] GetFilesInDirectory(string pathToDirectory, string pattern);
        string[] GetFilesInDirectory(string pathToDirectory, string pattern, bool recursive);
        bool IsFileExists(string filePath);
        byte[]? ReadAllBytes(string filePath);
        void WriteAllBytes(string filePath, byte[] data);
        string GetTempPath(string basePath);
        DateTime GetModificationTime(string filePath);
        long GetFileSize(string filePath);
        char DirectorySeparatorChar();
    }
}
