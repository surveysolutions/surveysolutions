﻿using System;
using System.IO;

namespace WB.Core.Infrastructure.FileSystem
{
    public interface IFileSystemAccessor
    {
        string CombinePath(string path1, string path2);
        string GetFileName(string filePath);
        long GetFileSize(string filePath);
        DateTime GetCreationTime(string filePath);
        bool IsDirectoryExists(string pathToDirectory);
        void CreateDirectory(string path);
        void DeleteDirectory(string path);

        bool IsFileExists(string pathToFile);
        Stream OpenOrCreateFile(string pathToFile);
        void DeleteFile(string pathToFile);

        string MakeValidFileName(string name);

        string[] GetDirectoriesInDirectory(string pathToDirectory);
        string[] GetFilesInDirectory(string pathToDirectory);

        void WriteAllText(string pathToFile, string content);
        void WriteAllBytes(string pathToFile, byte[] content);
        byte[] ReadAllBytes(string pathToFile);
        void CopyFileOrDirectory(string sourceDir, string targetDir);
    }
}
