using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Ionic.Zip;
using WB.Core.Infrastructure.FileSystem;

namespace WB.Core.Infrastructure.Files.Implementation.FileSystem
{
    internal class ZipArchiveUtils : IArchiveUtils
    {
        private readonly IFileSystemAccessor fileSystemAccessor;
        
        public ZipArchiveUtils(IFileSystemAccessor fileSystemAccessor)
        {
            this.fileSystemAccessor = fileSystemAccessor;
        }

        public void ZipDirectory(string directory, string archiveFile)
        {
            if (fileSystemAccessor.IsFileExists(archiveFile))
                throw new InvalidOperationException("zip file exists");

            using (var zipFile = new ZipFile()
                {
                    ParallelDeflateThreshold = -1,
                    AlternateEncoding = System.Text.Encoding.UTF8,
                    AlternateEncodingUsage = ZipOption.Always
                })
            {
                zipFile.AddDirectory(directory, fileSystemAccessor.GetFileName(directory));
                zipFile.Save(archiveFile);
            }
        }

        public void Unzip(string archivedFile, string extractToFolder)
        {
            using (ZipFile decompress = ZipFile.Read(archivedFile))
            {
                foreach (ZipEntry e in decompress)
                {
                    e.Extract(extractToFolder, ExtractExistingFileAction.OverwriteSilently);
                }
            }
        }
    }
}
