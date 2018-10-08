using System;
using System.Collections.Generic;
using System.IO;
using System.IO.Compression;
using ICSharpCode.SharpZipLib.Zip;
using WB.Services.Infrastructure.FileSystem;
using ZipFile = ICSharpCode.SharpZipLib.Zip.ZipFile;

namespace WB.Services.Export.Utils
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
                using (var archive = CreateArchive(archiveFile, archivePassword, CompressionLevel.Fastest))
                {

                    foreach (var file in Directory.EnumerateFiles(exportTempDirectoryPath))
                    {
                        archive.CreateEntry(file.Substring(exportTempDirectoryPath.Length + 1),
                            File.ReadAllBytes(file));
                    }
                }
            }

            //using (var zipFile = new ZipFile
            //{
            //    ParallelDeflateThreshold = -1,
            //    AlternateEncoding = Encoding.UTF8,
            //    AlternateEncodingUsage = ZipOption.Always,
            //    UseZip64WhenSaving = Zip64Option.AsNecessary
            //})
            //{
            //    if (password != null)
            //        zipFile.Password = password;

            //    zipFile.AddDirectory(directory, "");

            //    if (progress != null)
            //    {
            //        zipFile.SaveProgress += (o, e) =>
            //        {
            //            if (e.EventType == ZipProgressEventType.Saving_AfterWriteEntry)
            //                progress.Report(e.EntriesSaved * 100 / e.EntriesTotal);
            //        };
            //    }

            //    zipFile.Save(archiveFile);
            //}
        }

        public void ZipFiles(string exportTempDirectoryPath, IEnumerable<string> files, string archiveFilePath, string password = null)
        {
            using (var archiveFile = File.Create(archiveFilePath))
            {
                using (var archive = CreateArchive(archiveFile, password, CompressionLevel.Fastest))
                {

                    foreach (var file in files)
                    {
                        archive.CreateEntry(file.Substring(exportTempDirectoryPath.Length + 1),
                            File.ReadAllBytes(file));
                    }
                }
            }
        }
    }
}
