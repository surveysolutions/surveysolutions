using System;
using System.Collections.Generic;
using System.IO;
using System.Runtime.InteropServices.ComTypes;
using System.Text;
using Ionic.Zip;
using Ionic.Zlib;
using WB.Core.Infrastructure.FileSystem;

namespace WB.Infrastructure.Native.Files.Implementation.FileSystem
{
    public class ZipArchiveUtils : IArchiveUtils, IProtectedArchiveUtils
    {
        public IZipArchive CreateArchive(Stream outputStream, string password)
        {
            return new IonicZipArchive(outputStream, password);
        }

        public byte[] CompressStream(Stream uncompressedDataStream, string entryName)
        {
            byte[] compressedBytes;
            using (MemoryStream memoryStream = new MemoryStream())
            {
                using (ZipFile zip = new ZipFile())
                {
                    zip.AddEntry(entryName, uncompressedDataStream);
                    zip.Save(memoryStream);
                }

                compressedBytes = memoryStream.ToArray();
            }
            return compressedBytes;
        }

        public byte[] CompressStream(IEnumerable<ExtractedFile> entities)
        {
            byte[] compressedBytes;
            using (MemoryStream memoryStream = new MemoryStream())
            {
                using (ZipFile zip = new ZipFile())
                {
                    foreach (var entity in entities)
                    {
                        zip.AddEntry(entity.Name, entity.Bytes);
                    }
                    zip.Save(memoryStream);
                }

                compressedBytes = memoryStream.ToArray();
            }
            return compressedBytes;
        }
        
        public string CompressStream(string newZipPath, IEnumerable<string> paths)
        {
            using (ZipFile zip = new ZipFile())
            {
                
                zip.AddFiles(paths, "");
                zip.Save(newZipPath);
            }
            return newZipPath;
        }


        public void ZipDirectory(string directory, string archiveFile, string password, IProgress<int> progress = null)
        {
            using (var zipFile = new ZipFile
            {
                ParallelDeflateThreshold = -1,
                AlternateEncoding = Encoding.UTF8,
                AlternateEncodingUsage = ZipOption.Always,
                UseZip64WhenSaving = Zip64Option.AsNecessary
            })
            {
                if (password != null)
                    zipFile.Password = password;

                zipFile.AddDirectory(directory, "");

                if (progress != null)
                {
                    zipFile.SaveProgress += (o, e) =>
                    {
                        if (e.EventType == ZipProgressEventType.Saving_AfterWriteEntry)
                            progress.Report(e.EntriesSaved * 100 / e.EntriesTotal);
                    };
                }

                zipFile.Save(archiveFile);
            }
        }

        public void ZipDirectoryToFile(string sourceDirectory, string archiveFilePath)
            => this.ZipDirectory(sourceDirectory, archiveFilePath, null);

        public void ZipFiles(IEnumerable<string> files, string archiveFilePath, string password)
        {
            using (var zip = new ZipFile
            {
                CompressionLevel = CompressionLevel.Default,
                UseZip64WhenSaving = Zip64Option.AsNecessary
            })
            {
                if (password != null)
                    zip.Password = password;

                zip.AddFiles(files, "");
                zip.Save(archiveFilePath);
            }
        }

        public void ZipFiles(IEnumerable<string> files, string archiveFilePath)
        {
            ZipFiles(files, archiveFilePath, password: null);
        }

        public void Unzip(string archivedFile, string extractToFolder, bool ignoreRootDirectory = false)
        {
            using (ZipFile decompress = ZipFile.Read(archivedFile))
            {
                decompress.ExtractAll(extractToFolder, ExtractExistingFileAction.OverwriteSilently);
            }
        }

        public void Unzip(Stream fileStream, string extractToFolder, bool ignoreRootDirectory = false)
        {
            fileStream.Seek(0, SeekOrigin.Begin);
            using (ZipFile decompress = ZipFile.Read(fileStream))
            {
                decompress.ExtractAll(extractToFolder, ExtractExistingFileAction.OverwriteSilently);
            }
        }

        public IEnumerable<ExtractedFile> GetFilesFromArchive(Stream inputStream)
        {
            inputStream.Seek(0, SeekOrigin.Begin);
            
            using (ZipFile zip = ZipFile.Read(inputStream))
            {
                foreach (var zipEntry in zip.Entries)
                {
                    using (var memoryStream = new MemoryStream())
                    {
                        try
                        {
                            zipEntry.Extract(memoryStream);
                        }
                        catch (BadPasswordException ex)
                        {
                            throw new Core.Infrastructure.FileSystem.ZipException("Password required", ex);
                        }

                        yield return new ExtractedFile
                        {
                            Name = zipEntry.FileName,
                            Size = zipEntry.UncompressedSize,
                            Bytes = memoryStream.ToArray()
                        };
                    }
                }
            }
        }

        public IEnumerable<ExtractedFile> GetFilesFromArchive(byte[] archivedFileAsArray)
        {
            using (MemoryStream archivestream = new MemoryStream(archivedFileAsArray))
            using (ZipFile zip = ZipFile.Read(archivestream))
            {
                foreach (var zipEntry in zip.Entries)
                {
                    using (var memoryStream = new MemoryStream())
                    {
                        zipEntry.Extract(memoryStream);

                        yield return new ExtractedFile
                        {
                            Name = zipEntry.FileName,
                            Size = zipEntry.UncompressedSize,
                            Bytes = memoryStream.ToArray()
                        };
                    }
                }
            }
        }

        public ExtractedFile GetFileFromArchive(string archiveFilePath, string fileName)
        {
            if (string.IsNullOrEmpty(fileName)) throw new ArgumentNullException(nameof(fileName));
            
            using (var zips = ZipFile.Read(archiveFilePath))
            {
                foreach (var zip in zips)
                {
                    if (zip.IsDirectory) continue;
                    if (!zip.FileName.Contains(fileName)) continue;

                    using (var memoryStream = new MemoryStream())
                    {
                        zip.Extract(memoryStream);

                        return new ExtractedFile
                        {
                            Name = zip.FileName,
                            Size = zip.UncompressedSize,
                            Bytes = memoryStream.ToArray()
                        };
                    }
                }
            }

            return null;
        }
        
        public ExtractedStream GetFileFromArchive(Stream inputStream, string fileName)
        {
            inputStream.Seek(0, SeekOrigin.Begin);
            
            using (ZipFile zip = ZipFile.Read(inputStream))
            {
                foreach (var zipEntry in zip.Entries)
                {
                    if (zipEntry.IsDirectory) continue;
                    if (!zipEntry.FileName.Contains(fileName)) continue;

                    try
                    {
                        return new ExtractedStream
                        {
                            Name = zipEntry.FileName,
                            Size = zipEntry.UncompressedSize,
                            Content = zipEntry.OpenReader(),
                        };
                    }
                    catch (BadPasswordException ex)
                    {
                        throw new Core.Infrastructure.FileSystem.ZipException("Password required", ex);
                    }
                }
            }

            return null;
        }


        public ExtractedFile GetFileFromArchive(byte[] archivedFileAsArray, string fileName)
        {
            using (var archiveStream = new MemoryStream(archivedFileAsArray))
            {
                using (var zips = ZipFile.Read(archiveStream))
                {
                    foreach (var zip in zips)
                    {
                        if (zip.IsDirectory) continue;
                        if (!zip.FileName.Contains(fileName)) continue;

                        using (var memoryStream = new MemoryStream())
                        {
                            zip.Extract(memoryStream);

                            return new ExtractedFile
                            {
                                Name = zip.FileName,
                                Size = zip.UncompressedSize,
                                Bytes = memoryStream.ToArray()
                            };
                        }
                    }
                }
            }

            return null;
        }

        public Stream GetFileStream(string zipFilePath, string fileName)
        {
            using var zips = ZipFile.Read(zipFilePath);
            foreach (var zip in zips)
            {
                if (zip.IsDirectory) continue;
                if (!zip.FileName.Contains(fileName)) continue;

                return zip.InputStream;
            }

            return null;
        }

        public bool IsZipFile(string filePath)
        {
            return ZipFile.IsZipFile(filePath);
        }

        public bool IsZipStream(Stream zipStream)
        {
            zipStream.Seek(0, SeekOrigin.Begin);
            return ZipFile.IsZipFile(zipStream, false);
        }

        public Dictionary<string, long> GetArchivedFileNamesAndSize(string filePath)
        {
            var result = new Dictionary<string, long>();
            using (var zips = ZipFile.Read(filePath))
            {
                foreach (var zip in zips)
                {
                    if (zip.IsDirectory)
                        continue;
                    result.Add(zip.FileName, zip.UncompressedSize);
                }
            }
            return result;
        }

        public Dictionary<string, long> GetArchivedFileNamesAndSize(byte[] archivedFileAsArray)
        {
            var result = new Dictionary<string, long>();
            using (MemoryStream archivestream = new MemoryStream(archivedFileAsArray))
            using (ZipFile zips = ZipFile.Read(archivestream))
            {
                foreach (var zip in zips)
                {
                    if (zip.IsDirectory)
                        continue;
                    result.Add(zip.FileName, zip.UncompressedSize);
                }
            }
            return result;
        }

        public Dictionary<string, long> GetArchivedFileNamesAndSize(Stream inputStream)
        {
            inputStream.Seek(0, SeekOrigin.Begin);

            var result = new Dictionary<string, long>();
            using (ZipFile zips = ZipFile.Read(inputStream))
            {
                foreach (var zip in zips)
                {
                    if (zip.IsDirectory)
                        continue;
                    result.Add(zip.FileName, zip.UncompressedSize);
                }
            }
            return result;
        }

        public byte[] CompressStringToByteArray(string fileName, string fileContentAsString)
        {
            using (MemoryStream memoryStream = new MemoryStream())
            {
                using (ZipFile zip = new ZipFile())
                {
                    zip.AddEntry(fileName, Encoding.Unicode.GetBytes(fileContentAsString));
                    zip.Save(memoryStream);
                }

                return memoryStream.ToArray();
            }
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

        public void ProtectZipWithPassword(Stream inputZipStream, Stream protectedZipStream, string password)
        {
            using (var zipFile = ZipFile.Read(inputZipStream))
            {
                zipFile.Password = password;
                foreach (ZipEntry zipEntry in zipFile)
                {
                    zipEntry.Password = password;
                }
                zipFile.Save(protectedZipStream);
            }
            protectedZipStream.Position = 0;
        }
    }
}
