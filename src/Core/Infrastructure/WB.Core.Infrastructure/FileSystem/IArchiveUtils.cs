using System.Collections.Generic;

namespace WB.Core.Infrastructure.FileSystem
{
    public interface IArchiveUtils
    {
        void ZipDirectory(string directory, string archiveFile);
        byte[] ZipDirectoryToByteArray(string sourceDirectory, string directoryFilter = null, string fileFilter = null);
        void ZipFiles(IEnumerable<string> files, string archiveFilePath);
        void Unzip(string archivedFile, string extractToFolder);
        bool IsZipFile(string filePath);
        Dictionary<string, long> GetArchivedFileNamesAndSize(string filePath);
        string CompressString(string stringToCompress);
        string DecompressString(string stringToDecompress);
    }
}
