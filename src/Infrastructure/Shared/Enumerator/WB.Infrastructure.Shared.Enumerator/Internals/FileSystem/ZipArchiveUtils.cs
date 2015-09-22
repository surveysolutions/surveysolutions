using System;
using System.Collections.Generic;
using System.IO;
using System.IO.Compression;
using System.Text;
using ICSharpCode.SharpZipLib.Core;
using ICSharpCode.SharpZipLib.Zip;
using WB.Core.Infrastructure.FileSystem;

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


        public void Unzip(string archivedFile, string extractToFolder)
        {
            extractToFolder = Path.Combine(extractToFolder, Path.GetFileNameWithoutExtension(archivedFile));
            if (!Directory.Exists(extractToFolder))
            {
                Directory.CreateDirectory(extractToFolder);
            }

            using (FileStream fr = File.OpenRead(archivedFile))
            {
                using (ZipInputStream ins = new ZipInputStream(fr))
                {
                    ZipEntry ze = ins.GetNextEntry();
                    while (ze != null)
                    {
                        if (ze.IsDirectory)
                        {
                            Directory.CreateDirectory(Path.Combine(extractToFolder, ze.Name));
                        }
                        else if (ze.IsFile)
                        {
                            var targetDirectory = Path.Combine(extractToFolder,  Path.GetDirectoryName(ze.Name));
                            if (!Directory.Exists(targetDirectory))
                            {
                                Directory.CreateDirectory(targetDirectory);
                            }

                            FileStream fs = File.Create(Path.Combine(extractToFolder, ze.Name));

                            byte[] writeData = new byte[ze.Size];
                            int iteration = 0;
                            while (true)
                            {
                                int size = 2048;
                                size = ins.Read(writeData, (int)Math.Min(ze.Size, (iteration * 2048)),
                                    (int)Math.Min(ze.Size - (int)Math.Min(ze.Size, (iteration * 2048)), 2048));
                                if (size > 0)
                                {
                                    fs.Write(writeData, (int)Math.Min(ze.Size, (iteration * 2048)), size);
                                }
                                else
                                {
                                    break;
                                }
                                iteration++;
                            }
                            fs.Close();
                        }
                        ze = ins.GetNextEntry();
                    }
                }
            }
        }

        public bool IsZipFile(string filePath)
        {
            var zip = new ZipFile(filePath);
            return zip.TestArchive(true);
        }

        public Dictionary<string, long> GetArchivedFileNamesAndSize(string filePath)
        {
            return new Dictionary<string, long>();
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