using System.Collections.Generic;

namespace WB.Core.Infrastructure.FileSystem
{
    public interface IArchiveUtils
    {
        void ZipDirectory(string directory, string archiveFile);
        void ZipFiles(IEnumerable<string> files, IEnumerable<string> directories, string archiveFilePath);
        void Unzip(string archivedFile, string extractToFolder);
        bool IsZipFile(string filePath);
        Dictionary<string, long> GetArchivedFileNamesAndSize(string filePath);
        string CompressString(string stringToCompress);
        string DecompressString(string stringToDecompress);
    }
}
