using System.Collections.Generic;
using System.IO;
using System.Threading.Tasks;

namespace WB.Core.Infrastructure.FileSystem
{
    public interface IArchiveUtils
    {
        void ZipDirectory(string directory, string archiveFile);
        void ZipDirectoryToFile(string sourceDirectory, string archiveFilePath, string directoryFilter = null, string fileFilter = null);
        Task ZipDirectoryToFileAsync(string sourceDirectory, string archiveFilePath, string directoryFilter = null, string fileFilter = null);
        void ZipFiles(IEnumerable<string> files, string archiveFilePath);
        void Unzip(string archivedFile, string extractToFolder, bool ignoreRootDirectory = false);
        Task UnzipAsync(string archivedFile, string extractToFolder, bool ignoreRootDirectory = false);
        IEnumerable<UnzippedFile> UnzipStream(Stream zipStream);
        bool IsZipFile(string filePath);

        bool IsZipStream(Stream zipStream);
        Dictionary<string, long> GetArchivedFileNamesAndSize(string filePath);
        byte[] CompressStringToByteArray(string fileName, string fileContentAsString);
        string CompressString(string stringToCompress);
        string DecompressString(string stringToDecompress);
        Stream GetZipWithPassword(Stream inputZipStream, string password);
    }

    public class UnzippedFile
    {
        public string FileName { get; set; }
        public byte[] FileBytes { get; set; }
    }
}
