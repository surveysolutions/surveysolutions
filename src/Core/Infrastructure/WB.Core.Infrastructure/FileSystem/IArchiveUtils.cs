using System.Collections.Generic;
using System.IO;

namespace WB.Core.Infrastructure.FileSystem
{
    public interface IArchiveUtils
    {
        void ZipDirectoryToFile(string sourceDirectory, string archiveFilePath);
        void Unzip(string archivedFile, string extractToFolder, bool ignoreRootDirectory = false);

        void Unzip(byte[] archivedFileByteArray, string extractToFolder);

        bool IsZipFile(string filePath);

        Dictionary<string, long> GetArchivedFileNamesAndSize(string filePath);
        byte[] CompressStringToByteArray(string fileName, string fileContentAsString);
        string CompressString(string stringToCompress);
        string DecompressString(string stringToDecompress);
    }
}
