﻿using System;
using System.IO;
using System.IO.Compression;
using System.Threading.Tasks;
using WB.Core.Infrastructure.FileSystem;
using WB.Core.SharedKernels.Enumerator.Services;

namespace WB.Core.SharedKernels.Enumerator.Implementation.Utils
{
    public class ZipArchiveUtils : IEnumeratorArchiveUtils
    {
        readonly IFileSystemAccessor fileSystemAccessor;

        public ZipArchiveUtils(IFileSystemAccessor fileSystemAccessor)
        {
            this.fileSystemAccessor = fileSystemAccessor;
        }

        
        public void ZipDirectoryToFile(string sourceDirectory, string archiveFilePath, string fileFilter = null)
        {
            if (fileSystemAccessor.IsFileExists(archiveFilePath))
            {
                throw new InvalidOperationException("zip file exists");
            }

            ZipFile.CreateFromDirectory(sourceDirectory, archiveFilePath);
        }

        public Task ZipDirectoryToFileAsync(string sourceDirectory, string archiveFilePath, string fileFilter = null)
        {
            return Task.Run(() => ZipDirectoryToFile(sourceDirectory, archiveFilePath, fileFilter));
        }
        

        public void Unzip(string archivedFile, string extractToFolder, bool ignoreRootDirectory = false)
        {
            if (!ignoreRootDirectory)
            {
                extractToFolder = Path.Combine(extractToFolder, Path.GetFileNameWithoutExtension(archivedFile));
                if (!Directory.Exists(extractToFolder))
                {
                    Directory.CreateDirectory(extractToFolder);
                }
            }

            ZipFile.ExtractToDirectory(archivedFile, extractToFolder);
        }

        public Task UnzipAsync(string archivedFile, string extractToFolder, bool ignoreRootDirectory = false)
        {
            return Task.Run(() => Unzip(archivedFile, extractToFolder, ignoreRootDirectory));
        }
        
    }
}