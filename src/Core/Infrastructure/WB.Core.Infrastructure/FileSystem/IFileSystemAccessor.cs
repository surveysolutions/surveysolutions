using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace WB.Core.Infrastructure.FileSystem
{
    public interface IFileSystemAccessor
    {
        string CombinePath(params string[] pathParts);
        string GetFileName(string filePath);
        long GetFileSize(string filePath);
        bool IsDirectoryExists(string pathToDirectory);
        void CreateDirectory(string path);
        void DeleteDirectory(string path);

        bool IsFileExists(string pathToFile);
        void DeleteFile(string pathToFile);

        string MakeValidFileName(string name);

        string[] GetDirectoriesInDirectory(string pathToDirectory);
        string[] GetFilesInDirectory(string pathToDirectory);

        void WriteAllText(string pathToFile, string content);
        byte[] ReadAllBytes(string pathToFile);
    }
}
