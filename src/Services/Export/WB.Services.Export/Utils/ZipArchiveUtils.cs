﻿using System;
using System.Collections.Generic;
using System.IO;
using System.IO.Compression;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using WB.Services.Infrastructure.FileSystem;

namespace WB.Services.Export
{
    public class ZipArchiveUtils : IArchiveUtils
    {
        public void ZipDirectoryToFile(string sourceDirectory, string archiveFilePath)
        {
            throw new NotImplementedException();
        }

        public void Unzip(string archivedFile, string extractToFolder, bool ignoreRootDirectory = false)
        {
            throw new NotImplementedException();
        }

        public IEnumerable<ExtractedFile> GetFilesFromArchive(byte[] archivedFileAsArray)
        {
            throw new NotImplementedException();
        }

        public bool IsZipFile(string filePath)
        {
            throw new NotImplementedException();
        }

        public bool IsZipStream(Stream zipStream)
        {
            throw new NotImplementedException();
        }

        public Dictionary<string, long> GetArchivedFileNamesAndSize(string filePath)
        {
            throw new NotImplementedException();
        }

        public Dictionary<string, long> GetArchivedFileNamesAndSize(byte[] archivedFileAsArray)
        {
            throw new NotImplementedException();
        }

        public Dictionary<string, long> GetArchivedFileNamesAndSize(Stream inpuStream)
        {
            throw new NotImplementedException();
        }

        public byte[] CompressStringToByteArray(string fileName, string fileContentAsString)
        {
            throw new NotImplementedException();
        }

        public string CompressString(string stringToCompress)
        {
            throw new NotImplementedException();
        }

        public string DecompressString(string stringToDecompress)
        {
            throw new NotImplementedException();
        }

        public IEnumerable<ExtractedFile> GetFilesFromArchive(Stream inputStream)
        {
            throw new NotImplementedException();
        }

        public IZipArchive CreateArchive(Stream outputStream, string password, CompressionLevel compressionLevel)
        {
            return new IonicZipArchive(outputStream, password);
        }

        public void ZipDirectory(string exportTempDirectoryPath, string archiveName, string archivePassword, IProgress<int> exportProgress)
        {
            using (var archiveFile = File.Create(archiveName))
            {
                using (var archive = CreateArchive(archiveFile, archivePassword, CompressionLevel.Optimal))
                {
                    foreach (var file in Directory.EnumerateFiles(exportTempDirectoryPath))
                    {
                        using (var fs = File.OpenRead(file))
                        {
                            if (fs.Length == 0)
                            {
                                archive.CreateEntry(file.Substring(exportTempDirectoryPath.Length + 1), Array.Empty<byte>());
                            }
                            else
                            {
                                archive.CreateEntry(file.Substring(exportTempDirectoryPath.Length + 1), fs);
                            }
                        }
                    }
                }
            }
        }

        public async Task ZipDirectoryAsync(string exportTempDirectoryPath, string archiveName,
            string archivePassword, 
            IProgress<int> exportProgress,
            CancellationToken token = default)
        {
            using (var archiveFile = File.Create(archiveName))
            {
                using (var archive = CreateArchive(archiveFile, archivePassword, CompressionLevel.Optimal))
                {
                    var files = Directory.EnumerateFiles(exportTempDirectoryPath).ToList();
                    var total = files.Count;
                    long added = 0;
                    foreach (var file in Directory.EnumerateFiles(exportTempDirectoryPath))
                    {
                        using (var fs = File.OpenRead(file))
                        {
                            if (fs.Length == 0)
                            {
                                await archive.CreateEntryAsync(file.Substring(exportTempDirectoryPath.Length + 1), Array.Empty<byte>(), token);
                            }
                            else
                            {
                                await archive.CreateEntryAsync(file.Substring(exportTempDirectoryPath.Length + 1), fs, token);
                            }
                        }

                        added++;
                        exportProgress?.Report(added.PercentOf(total));
                    }
                }
            }
        }

        public void ZipFiles(string exportTempDirectoryPath, IEnumerable<string> files, string archiveFilePath, string password = null)
        {
            using (var archiveFile = File.Create(archiveFilePath))
            {
                using (var archive = CreateArchive(archiveFile, password, CompressionLevel.Fastest))
                {
                    foreach (var file in files)
                    {
                        using (var fs = File.OpenRead(file))
                        {
                            if (fs.Length == 0)
                            {
                                archive.CreateEntry(file.Substring(exportTempDirectoryPath.Length + 1), Array.Empty<byte>());
                            }
                            else
                            {
                                archive.CreateEntry(file.Substring(exportTempDirectoryPath.Length + 1), fs);
                            }
                        }
                    }
                }
            }
        }
    }
}
