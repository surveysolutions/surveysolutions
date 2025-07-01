using System;
using System.Collections.Generic;
using System.IO;
using System.IO.Compression;
using System.Text;
using WB.Core.Infrastructure.FileSystem;

namespace WB.Infrastructure.Native.Files.Implementation.FileSystem
{
    public class ZipArchiveUtils : IArchiveUtils
    {
        public byte[] CompressContentToSingleFile(byte[] uncompressedData, string entityName)
        {
            using MemoryStream memoryStream = new MemoryStream();
            using (var archive = new ZipArchive(memoryStream, ZipArchiveMode.Create, true))
            {
                var entry = archive.CreateEntry(entityName, CompressionLevel.Optimal);
                using (var entryStream = entry.Open())
                {
                    entryStream.Write(uncompressedData, 0, uncompressedData.Length);
                }
            }
            
            var compressedBytes = memoryStream.ToArray();
            return compressedBytes;
        }

        public void CreateArchiveFromDirectory(string directory, string archiveFile)
        {
            if(File.Exists(archiveFile))
                File.Delete(archiveFile);
            
            ZipFile.CreateFromDirectory(directory, archiveFile, CompressionLevel.Optimal, false, Encoding.UTF8);
        }

        public void CreateArchiveFromFileList(IEnumerable<string> files, string archiveFilePath)
        {
            using (var fileStream = new FileStream(archiveFilePath, FileMode.Create, FileAccess.Write, FileShare.None))
            using (var archive = new ZipArchive(fileStream, ZipArchiveMode.Create, true))
            {
                foreach (var file in files)
                {
                    var entry = archive.CreateEntry(Path.GetFileName(file), CompressionLevel.Optimal);
                    using (var entryStream = entry.Open())
                    using (var entryFileStream = File.OpenRead(file))
                    {
                        entryFileStream.CopyTo(entryStream);
                    }
                }
            }
        }
        
        public void ExtractToDirectory(string archivedFile, string extractToFolder, bool ignoreRootDirectory = false)
        {
            if (archivedFile == null) throw new ArgumentNullException(nameof(archivedFile));

            if (!ignoreRootDirectory)
            {
                extractToFolder = Path.Combine(extractToFolder, Path.GetFileNameWithoutExtension(archivedFile));
                if (!Directory.Exists(extractToFolder))
                {
                    Directory.CreateDirectory(extractToFolder);
                }
            }

            ZipFile.ExtractToDirectory(archivedFile, extractToFolder);
        }

        public void ExtractToDirectory(Stream fileStream, string extractToFolder, bool ignoreRootDirectory = false)
        {
            fileStream.Seek(0, SeekOrigin.Begin);
            
            ZipFile.ExtractToDirectory(fileStream, extractToFolder);
        }

        public IList<ExtractedFile> GetFilesFromArchive(Stream inputStream)
        {
            inputStream.Seek(0, SeekOrigin.Begin);

            List<ExtractedFile> result = new List<ExtractedFile>();

            using (var zip = new ZipArchive(inputStream, ZipArchiveMode.Read, true))
            {
                foreach (var zipEntry in zip.Entries)
                {
                    if (zipEntry.IsDirectory()) continue;

                    result.Add(new ExtractedFile
                    {
                        Name = zipEntry.FullName,
                        Size = zipEntry.Length,
                        Bytes = zipEntry.GetContent()
                    });
                }
            }

            return result;
        }

        public ExtractedFile GetFileFromArchive(string archiveFilePath, string fileName)
        {
            if (string.IsNullOrEmpty(fileName)) throw new ArgumentNullException(nameof(fileName));
            
            using (var archive = ZipFile.OpenRead(archiveFilePath))
            {
                foreach (var entry in archive.Entries)
                {                    
                    if (entry.IsDirectory()) continue;
                    
                    if (entry.FullName.Contains(fileName))
                    {
                        return new ExtractedFile
                        {
                            Name = entry.FullName,
                            Size = entry.Length,
                            Bytes = entry.GetContent()
                        };
                    }
                }
            }
            
            return null;
        }
        
        public ExtractedFile GetFileFromArchive(byte[] archivedFileAsArray, string fileName)
        {
            using (var archiveStream = new MemoryStream(archivedFileAsArray))
            using (var zip = new ZipArchive(archiveStream, ZipArchiveMode.Read, true))
            {
                foreach (var zipEntry in zip.Entries)
                {
                    if (zipEntry.IsDirectory()) continue;
                    
                    if (zipEntry.FullName.Contains(fileName))
                    {
                        return new ExtractedFile
                        {
                            Name = zipEntry.FullName,
                            Size = zipEntry.Length,
                            Bytes = zipEntry.GetContent()
                        };
                    }
                }
            }
            
            return null;
        }

        public bool IsZipStream(Stream zipStream)
        {
            zipStream.Seek(0, SeekOrigin.Begin);

            try
            {
                using var zip = new ZipArchive(zipStream, ZipArchiveMode.Read, true);
                return true;
            }
            catch (InvalidDataException)
            {
                return false;
            }
        }

        public Dictionary<string, long> GetFileNamesAndSizesFromArchive(byte[] archivedFileAsArray)
        {
            using var archiveStream = new MemoryStream(archivedFileAsArray);
            return GetFileNamesAndSizesFromArchive(archiveStream);
        }

        public Dictionary<string, long> GetFileNamesAndSizesFromArchive(Stream inputStream)
        {
            var result = new Dictionary<string, long>();
            string destDirectory = Path.GetFullPath(Path.GetTempPath());
            
            using var zip = new ZipArchive(inputStream, ZipArchiveMode.Read, true);
            foreach (var entry in zip.Entries)
            {
                if (entry.IsDirectory()) continue;

                string destFileName = Path.GetFullPath(Path.Combine(destDirectory, entry.FullName));
                string fullDestDirPath = Path.GetFullPath(destDirectory + Path.DirectorySeparatorChar);
                if (!destFileName.StartsWith(fullDestDirPath)) {
                    throw new System.InvalidOperationException("Entry is outside the target dir: " + destFileName);
                }
                
                result.Add(entry.FullName, entry.Length);
            }

            return result;
        }

        public string CompressString(string stringToCompress)
        {
            var bytes = Encoding.Unicode.GetBytes(stringToCompress);
            using (var msi = new MemoryStream(bytes))
            using (var mso = new MemoryStream())
            {
                using (var gs = new GZipStream(mso, CompressionMode.Compress))
                {
                    msi.CopyTo(gs);
                }
                return Convert.ToBase64String(mso.ToArray());
            }
        }

        public string DecompressString(string stringToDecompress)
        {
            var bytes = Convert.FromBase64String(stringToDecompress);
            using (var msi = new MemoryStream(bytes))
            using (var mso = new MemoryStream())
            {
                using (var gs = new GZipStream(msi, CompressionMode.Decompress))
                {
                    gs.CopyTo(mso);
                }
                return Encoding.Unicode.GetString(mso.ToArray());
            }
        }
    }
}
