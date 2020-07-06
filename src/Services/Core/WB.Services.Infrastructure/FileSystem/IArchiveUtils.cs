using System;
using System.Collections.Generic;
using System.IO;
using System.Threading;
using System.Threading.Tasks;

namespace WB.Services.Infrastructure.FileSystem
{
    public interface IArchiveUtils
    {
        void ZipDirectoryToFile(string sourceDirectory, string archiveFilePath);
        void Unzip(string archivedFile, string extractToFolder, bool ignoreRootDirectory = false);
        
        IEnumerable<ExtractedFile> GetFilesFromArchive(byte[] archivedFileAsArray);

        bool IsZipFile(string filePath);
        bool IsZipStream(Stream zipStream);

        Dictionary<string, long> GetArchivedFileNamesAndSize(string filePath);
        Dictionary<string, long> GetArchivedFileNamesAndSize(byte[] archivedFileAsArray);
        Dictionary<string, long> GetArchivedFileNamesAndSize(Stream inpuStream);
        byte[] CompressStringToByteArray(string fileName, string fileContentAsString);
        string CompressString(string stringToCompress);
        string DecompressString(string stringToDecompress);
        IEnumerable<ExtractedFile> GetFilesFromArchive(Stream inputStream);
        IZipArchive CreateArchive(Stream outputStream, string? password, System.IO.Compression.CompressionLevel compressionLevel);
        void ZipDirectory(string exportTempDirectoryPath, string archiveName, string archivePassword, IProgress<int> exportProgress);

        Task ZipDirectoryAsync(string exportTempDirectoryPath, string archiveName,
            string? archivePassword,
            IProgress<int> exportProgress,
            CancellationToken token = default);
        void ZipFiles(string exportTempDirectoryPath, IEnumerable<string> files, string archiveFilePath, string password = "");
    }
}
