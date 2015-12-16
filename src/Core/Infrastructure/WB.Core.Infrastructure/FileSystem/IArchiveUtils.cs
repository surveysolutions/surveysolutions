using System.Collections.Generic;
using System.IO;

namespace WB.Core.Infrastructure.FileSystem
{
    public interface IArchiveUtils
    {
        void ZipDirectory(string directory, string archiveFile);
        byte[] ZipDirectoryToByteArray(string sourceDirectory, string directoryFilter = null, string fileFilter = null);
        void ZipFiles(IEnumerable<string> files, string archiveFilePath);
        void Unzip(string archivedFile, string extractToFolder, bool ignoreRootDirectory = false);
        IEnumerable<UnzippedFile> UnzipStream(Stream zipStream);
        bool IsZipFile(string filePath);

        bool IsZipStream(Stream zipStream);
        Dictionary<string, long> GetArchivedFileNamesAndSize(string filePath);
        string CompressString(string stringToCompress);
        string DecompressString(string stringToDecompress);
    }

    public class UnzippedFile
    {
        public string FileName { get; set; }
        public Stream FileStream { get; set; }
    }
}
