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
        public byte[] CompressContentToEntity(byte[] uncompressedData, string entityName)
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

        public void ZipDirectory(string directory, string archiveFile)
        {
            ZipFile.CreateFromDirectory(directory, archiveFile, CompressionLevel.Optimal, false, Encoding.UTF8);
            
            // using (var zipFile = new ZipFile
            // {
            //     ParallelDeflateThreshold = -1,
            //     AlternateEncoding = Encoding.UTF8,
            //     AlternateEncodingUsage = ZipOption.Always,
            //     UseZip64WhenSaving = Zip64Option.AsNecessary
            // })
            // {
            //     zipFile.AddDirectory(directory, "");
            //     zipFile.Save(archiveFile);
            // }
        }

        public void ZipFiles(IEnumerable<string> files, string archiveFilePath)
        {
            using (var archive = new ZipArchive(File.Create(archiveFilePath), ZipArchiveMode.Create, true))
            {
                foreach (var file in files)
                {
                    var entry = archive.CreateEntry(Path.GetFileName(file), CompressionLevel.Optimal);
                    using (var entryStream = entry.Open())
                    {
                        using (var fileStream = File.OpenRead(file))
                        {
                            fileStream.CopyTo(entryStream);
                        }
                    }
                }
            }
            
            // using (var zip = new ZipFile
            // {
            //     CompressionLevel = CompressionLevel.Default,
            //     UseZip64WhenSaving = Zip64Option.AsNecessary,
            //     AlternateEncoding = Encoding.UTF8,
            //     AlternateEncodingUsage = ZipOption.AsNecessary,
            // })
            // {
            //     zip.AddFiles(files, "");
            //     zip.Save(archiveFilePath);
            // }
        }
        
        public void Unzip(string archivedFile, string extractToFolder, bool ignoreRootDirectory = false)
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

        public void Unzip(Stream fileStream, string extractToFolder, bool ignoreRootDirectory = false)
        {
            fileStream.Seek(0, SeekOrigin.Begin);
            
            ZipFile.ExtractToDirectory(fileStream, extractToFolder);
        }

        public IEnumerable<ExtractedFile> GetFilesFromArchive(Stream inputStream)
        {
            inputStream.Seek(0, SeekOrigin.Begin);
            
            using var zip = new ZipArchive(inputStream, ZipArchiveMode.Read);
            foreach (var zipEntry in zip.Entries)
            {
                if (zipEntry.FullName.EndsWith("/")) continue;
                
                using (var memoryStream = new MemoryStream())
                {
                    zipEntry.Open().CopyTo(memoryStream);
                    yield return new ExtractedFile
                    {
                        Name = zipEntry.FullName,
                        Size = zipEntry.Length,
                        Bytes = memoryStream.ToArray()
                    };
                }
            }
            
            // using (ZipFile zip = ZipFile.Read(inputStream))
            // {
            //     foreach (var zipEntry in zip.Entries)
            //     {
            //         using (var memoryStream = new MemoryStream())
            //         {
            //             try
            //             {
            //                 zipEntry.Extract(memoryStream);
            //             }
            //             catch (BadPasswordException ex)
            //             {
            //                 throw new Core.Infrastructure.FileSystem.ZipException("Password required", ex);
            //             }
            //
            //             yield return new ExtractedFile
            //             {
            //                 Name = zipEntry.FileName,
            //                 Size = zipEntry.UncompressedSize,
            //                 Bytes = memoryStream.ToArray()
            //             };
            //         }
            //     }
            // }
        }

        public ExtractedFile GetFileFromArchive(string archiveFilePath, string fileName)
        {
            if (string.IsNullOrEmpty(fileName)) throw new ArgumentNullException(nameof(fileName));
            
            using (var archive = ZipFile.OpenRead(archiveFilePath))
            {
                foreach (var entry in archive.Entries)
                {
                    // if entry is a Directory  continue iteration
                    if (entry.FullName.EndsWith("/")) continue;
                    
                    if (entry.FullName.Contains(fileName))
                    {
                        using (var memoryStream = new MemoryStream())
                        {
                            entry.Open().CopyTo(memoryStream);
                            return new ExtractedFile
                            {
                                Name = entry.FullName,
                                Size = entry.Length,
                                Bytes = memoryStream.ToArray()
                            };
                        }
                    }
                }
            }
            
            // using (var zips = ZipFile.Read(archiveFilePath))
            // {
            //     foreach (var zip in zips)
            //     {
            //         if (zip.IsDirectory) continue;
            //         if (!zip.FileName.Contains(fileName)) continue;
            //
            //         using (var memoryStream = new MemoryStream())
            //         {
            //             zip.Extract(memoryStream);
            //
            //             return new ExtractedFile
            //             {
            //                 Name = zip.FileName,
            //                 Size = zip.UncompressedSize,
            //                 Bytes = memoryStream.ToArray()
            //             };
            //         }
            //     }
            // }
            
            return null;
        }
        
        public ExtractedFile GetFileFromArchive(byte[] archivedFileAsArray, string fileName)
        {
            using (var archiveStream = new MemoryStream(archivedFileAsArray))
            using (var zip = new ZipArchive(archiveStream, ZipArchiveMode.Read))
            {
                foreach (var zipEntry in zip.Entries)
                {
                    if (zipEntry.FullName.EndsWith("/")) continue;
                    if (zipEntry.FullName.Contains(fileName))
                    {
                        using (var memoryStream = new MemoryStream())
                        {
                            zipEntry.Open().CopyTo(memoryStream);
                            return new ExtractedFile
                            {
                                Name = zipEntry.FullName,
                                Size = zipEntry.Length,
                                Bytes = memoryStream.ToArray()
                            };
                        }
                    }
                }
            }

            // using (var archiveStream = new MemoryStream(archivedFileAsArray))
            // {
            //     using (var zips = ZipFile.Read(archiveStream))
            //     {
            //         foreach (var zip in zips)
            //         {
            //             if (zip.IsDirectory) continue;
            //             if (!zip.FileName.Contains(fileName)) continue;
            //
            //             using (var memoryStream = new MemoryStream())
            //             {
            //                 zip.Extract(memoryStream);
            //
            //                 return new ExtractedFile
            //                 {
            //                     Name = zip.FileName,
            //                     Size = zip.UncompressedSize,
            //                     Bytes = memoryStream.ToArray()
            //                 };
            //             }
            //         }
            //     }
            // }
            //
            
            return null;
        }

        public bool IsZipStream(Stream zipStream)
        {
            zipStream.Seek(0, SeekOrigin.Begin);

            try
            {
                using var zip = new ZipArchive(zipStream, ZipArchiveMode.Read);
                return true;
            }
            catch (InvalidDataException)
            {
                return false;
            }
        }

        public Dictionary<string, long> GetArchivedFileNamesAndSize(byte[] archivedFileAsArray)
        {
            using var archiveStream = new MemoryStream(archivedFileAsArray);
            return GetArchivedFileNamesAndSize(archiveStream);
        }

        public Dictionary<string, long> GetArchivedFileNamesAndSize(Stream inputStream)
        {
            var result = new Dictionary<string, long>();

            using var zip = new ZipArchive(inputStream, ZipArchiveMode.Read);
            foreach (var zipEntry in zip.Entries)
            {
                if (zipEntry.FullName.EndsWith("/")) continue;
                result.Add(zipEntry.FullName, zipEntry.Length);
            }

            // using (MemoryStream archivestream = new MemoryStream(archivedFileAsArray))
            // using (ZipFile zips = ZipFile.Read(archivestream))
            // {
            //     foreach (var zip in zips)
            //     {
            //         if (zip.IsDirectory)
            //             continue;
            //         result.Add(zip.FileName, zip.UncompressedSize);
            //     }
            // }
            
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
