using System.Collections.Generic;
using System.IO;

namespace WB.Core.Infrastructure.FileSystem
{
    public interface IArchiveUtils
    {
        // Extraction methods
        void ExtractToDirectory(string archivedFile, string extractToFolder, bool ignoreRootDirectory = false);
        void ExtractToDirectory(Stream archivedFile, string extractToFolder, bool ignoreRootDirectory = false);
        ExtractedFile GetFileFromArchive(string archiveFilePath, string fileName);
        ExtractedFile GetFileFromArchive(byte[] archivedFileAsArray, string fileName);
        IList<ExtractedFile> GetFilesFromArchive(Stream inputStream);
        bool IsZipStream(Stream zipStream);

        // File information methods
        Dictionary<string, long> GetFileNamesAndSizesFromArchive(byte[] archivedFileAsArray);
        Dictionary<string, long> GetFileNamesAndSizesFromArchive(Stream inputStream);

        // Compression methods
        string CompressString(string stringToCompress);
        string DecompressString(string stringToDecompress);
        byte[] CompressContentToSingleFile(byte[] uncompressedData, string entryName);
        void CreateArchiveFromDirectory(string directory, string archiveFile);
        void CreateArchiveFromFileList(IEnumerable<string> files, string archiveFilePath);
    }
}
