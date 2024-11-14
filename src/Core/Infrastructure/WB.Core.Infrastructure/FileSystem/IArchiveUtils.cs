using System;
using System.Collections.Generic;
using System.IO;

namespace WB.Core.Infrastructure.FileSystem
{
    public interface IArchiveUtils
    {
        void Unzip(string archivedFile, string extractToFolder, bool ignoreRootDirectory = false);
        void Unzip(Stream archivedFile, string extractToFolder, bool ignoreRootDirectory = false);
        
        ExtractedFile GetFileFromArchive(string archiveFilePath, string fileName);
        ExtractedFile GetFileFromArchive(byte[] archivedFileAsArray, string fileName);
        
        bool IsZipStream(Stream zipStream);

        Dictionary<string, long> GetArchivedFileNamesAndSize(byte[] archivedFileAsArray);
        Dictionary<string, long> GetArchivedFileNamesAndSize(Stream inpuStream);
        
        string CompressString(string stringToCompress);
        string DecompressString(string stringToDecompress);
        IEnumerable<ExtractedFile> GetFilesFromArchive(Stream inputStream);
        byte[] CompressContentToEntity(byte[] uncompressedData, string entryName);
        
        void ZipDirectory(string directory, string archiveFile);
        void ZipFiles(IEnumerable<string> files, string archiveFilePath);
    }
}
