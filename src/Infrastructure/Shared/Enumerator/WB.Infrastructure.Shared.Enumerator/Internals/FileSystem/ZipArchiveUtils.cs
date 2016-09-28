using System;
using System.Collections.Generic;
using System.IO;
using System.IO.Compression;
using System.Text;
using System.Threading.Tasks;
using ICSharpCode.SharpZipLib.Core;
using ICSharpCode.SharpZipLib.Zip;
using Java.IO;
using WB.Core.GenericSubdomains.Portable;
using WB.Core.Infrastructure.FileSystem;
using File = System.IO.File;

namespace WB.Infrastructure.Shared.Enumerator.Internals.FileSystem
{
    class ZipArchiveUtils : IArchiveUtils
    {
        readonly IFileSystemAccessor fileSystemAccessor;

        public ZipArchiveUtils(IFileSystemAccessor fileSystemAccessor)
        {
            this.fileSystemAccessor = fileSystemAccessor;
        }

        public void ZipDirectory(string directory, string archiveFile)
        {
            if (fileSystemAccessor.IsFileExists(archiveFile))
            {
                throw new InvalidOperationException("zip file exists");
            }

            FastZip zip = new FastZip();
            zip.CreateZip(archiveFile, directory, true, string.Empty);
        }

        public void ZipDirectoryToFile(string sourceDirectory, string archiveFilePath, string directoryFilter = null, string fileFilter = null)
        {
            using (var fileOutputStream = this.fileSystemAccessor.OpenOrCreateFile(archiveFilePath, false))
            {
                FastZip zip = new FastZip();
                zip.CreateZip(fileOutputStream, sourceDirectory, true, fileFilter, directoryFilter);
            }
        }

        public Task ZipDirectoryToFileAsync(string sourceDirectory, string archiveFilePath, string directoryFilter = null, string fileFilter = null)
        {
            return Task.Run(() => ZipDirectoryToFile(sourceDirectory, archiveFilePath, directoryFilter, fileFilter));
        }

        public void ZipFiles(IEnumerable<string> files, string archiveFilePath)
        {
            using (FileStream fsOut = File.Create(archiveFilePath))
            {
                using (ZipOutputStream zipStream = new ZipOutputStream(fsOut))
                {
                    foreach (string filename in files)
                    {
                        FileInfo fileInfo = new FileInfo(filename);

                        var entryName = ZipEntry.CleanName(filename);
                        ZipEntry newEntry = new ZipEntry(entryName);
                        newEntry.DateTime = fileInfo.LastWriteTime;
                        newEntry.Size = fileInfo.Length;

                        zipStream.PutNextEntry(newEntry);

                        byte[] buffer = new byte[4096];
                        using (FileStream streamReader = File.OpenRead(filename))
                        {
                            StreamUtils.Copy(streamReader, zipStream, buffer);
                        }

                        zipStream.CloseEntry();
                    }
                    zipStream.IsStreamOwner = true;
                    zipStream.Close();
                }
            }
        }


        public void Unzip(string archivedFile, string extractToFolder, bool ignoreRootDirectory = false)
        {
            if (!ignoreRootDirectory)
            {
                extractToFolder = Path.Combine(extractToFolder, Path.GetFileNameWithoutExtension(archivedFile));
                if (!Directory.Exists(extractToFolder))
                {
                    Directory.CreateDirectory(extractToFolder);
                }
            }

            using (ZipInputStream zipFileStream = new ZipInputStream(File.OpenRead(archivedFile)))
            {
                ZipEntry zipFileOrDirectory;
                while ((zipFileOrDirectory = zipFileStream.GetNextEntry()) != null)
                {
                    if (zipFileOrDirectory.IsDirectory)
                    {
                        Directory.CreateDirectory(Path.Combine(extractToFolder, zipFileOrDirectory.Name));
                    }
                    else 
                    {
                        var targetDirectory = Path.Combine(extractToFolder, Path.GetDirectoryName(zipFileOrDirectory.Name));
                        if (!Directory.Exists(targetDirectory))
                        {
                            Directory.CreateDirectory(targetDirectory);
                        }

                        using (FileStream streamWriter = File.Create(Path.Combine(extractToFolder, zipFileOrDirectory.Name)))
                        {
                            int size = 2048;
                            byte[] data = new byte[size];
                            while (true)
                            {
                                size = zipFileStream.Read(data, 0, data.Length);
                                if (size > 0)
                                    streamWriter.Write(data, 0, size);
                                else
                                    break;
                            }
                        }
                    }
                }
            }
        }

        public Task UnzipAsync(string archivedFile, string extractToFolder, bool ignoreRootDirectory = false)
        {
            return Task.Run(() => Unzip(archivedFile, extractToFolder, ignoreRootDirectory));
        }

        public IEnumerable<UnzippedFile> UnzipStream(Stream zipStream)
        {
            zipStream.Seek(0, SeekOrigin.Begin);
            using (ZipInputStream zipFileStream = new ZipInputStream(zipStream))
            {
                ZipEntry zipFileOrDirectory;
                while ((zipFileOrDirectory = zipFileStream.GetNextEntry()) != null)
                {
                    if (!zipFileOrDirectory.IsFile) continue;

                    var unzippedFileStream = new MemoryStream();
                    zipFileStream.CopyTo(unzippedFileStream);
                    yield return new UnzippedFile
                    {
                        FileName = zipFileOrDirectory.Name,
                        FileBytes = unzippedFileStream.ToArray()
                    };
                }
            }
        }

        public bool IsZipFile(string filePath)
        {
            var zip = new ZipFile(filePath);
            return zip.TestArchive(true);
        }

        public bool IsZipStream(Stream zipStream)
        {
            var zip = new ZipFile(zipStream);
            return zip.TestArchive(true);
        }

        public Dictionary<string, long> GetArchivedFileNamesAndSize(string filePath)
        {
            return new Dictionary<string, long>();
        }


        public byte[] CompressStringToByteArray(string fileName, string fileContentAsString)
        {
            using (MemoryStream outputMemoryStream = new MemoryStream())
            {
                using (ZipOutputStream zipStream = new ZipOutputStream(outputMemoryStream))
                {
                    zipStream.SetLevel(3);

                    ZipEntry newEntry = new ZipEntry(fileName) { DateTime = DateTime.Now };

                    zipStream.PutNextEntry(newEntry);

                    var inputMemoryStream = new MemoryStream(Encoding.Unicode.GetBytes(fileContentAsString));

                    StreamUtils.Copy(inputMemoryStream, zipStream, new byte[4096]);
                    zipStream.CloseEntry();

                    zipStream.IsStreamOwner = false;
                }
                return outputMemoryStream.ToArray();
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

        public Stream GetZipWithPassword(Stream inputZipStream, string password)
        {
            throw new NotImplementedException();
        }
    }
}