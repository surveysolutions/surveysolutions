using System.Collections.Generic;
using System.IO;

namespace WB.Core.Infrastructure.FileSystem
{
    public interface IArchiveUtils
    {
        void ZipDirectoryToFile(string sourceDirectory, string archiveFilePath);
        void Unzip(string archivedFile, string extractToFolder, bool ignoreRootDirectory = false);
        void Unzip(Stream archivedFile, string extractToFolder, bool ignoreRootDirectory = false);
        
        IEnumerable<ExtractedFile> GetFilesFromArchive(byte[] archivedFileAsArray);
        ExtractedFile GetFileFromArchive(string archiveFilePath, string fileName);
        ExtractedStream GetFileFromArchive(Stream inputStream, string fileName);
        ExtractedFile GetFileFromArchive(byte[] archivedFileAsArray, string fileName);

        bool IsZipFile(string filePath);
        bool IsZipStream(Stream zipStream);

        Dictionary<string, long> GetArchivedFileNamesAndSize(string filePath);
        Dictionary<string, long> GetArchivedFileNamesAndSize(byte[] archivedFileAsArray);
        Dictionary<string, long> GetArchivedFileNamesAndSize(Stream inpuStream);
        byte[] CompressStringToByteArray(string fileName, string fileContentAsString);
        string CompressString(string stringToCompress);
        string DecompressString(string stringToDecompress);
        IEnumerable<ExtractedFile> GetFilesFromArchive(Stream inputStream);
        byte[] CompressStream(Stream uncompressedDataStream, string entryName);
        byte[] CompressStream(IEnumerable<ExtractedFile> entities);
        string CompressStream(string newZipPath, IEnumerable<string> paths);
    }
}
