using System.IO;

namespace WB.Services.Export.Infrastructure
{
    public interface IFileSystemAccessor
    {
        Stream OpenOrCreateFile(string filePath, bool append);
        void WriteAllText(string contentFilePath, string toString);
        string MakeValidFileName(string questionnaireTitle);
        string Combine(params string[] parts);
        string CombinePath(params string[] parts);
        bool IsDirectoryExists(string path);
        void DeleteDirectory(string path);
        void CreateDirectory(string path);
        string GetFileName(string path);
        void DeleteFile(string path);
        void MoveFile(string fromPath, string toPath);
        string[] GetFilesInDirectory(string directoryPath);
        bool IsFileExists(string filePath);
        byte[] ReadAllBytes(string filePath);
        void WriteAllBytes(string filePath, byte[] data);
        string GetTempPath(string basePath);
    }
}
