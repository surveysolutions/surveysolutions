using System;
using System.Collections.Generic;
using System.IO;
using System.IO.Compression;
using System.Text;
using ICSharpCode.SharpZipLib.Zip;
using WB.Services.Infrastructure.FileSystem;
using ZipFile = ICSharpCode.SharpZipLib.Zip.ZipFile;


namespace WB.Services.Export.Utils
{
    public static class DictionaryExtensions
    {
        public static TValue GetOrNull<TKey, TValue>(this IDictionary<TKey, TValue> dictionary, TKey key)
            where TValue : class
        {
            return dictionary.TryGetValue(key, out var value) ? value : null;
        }
    }

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
    }
}
