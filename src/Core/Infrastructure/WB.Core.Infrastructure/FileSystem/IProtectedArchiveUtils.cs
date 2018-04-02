using System;
using System.Collections.Generic;
using System.IO;

namespace WB.Core.Infrastructure.FileSystem
{
    public interface IProtectedArchiveUtils
    {
        IZipArchive CreateArchive(Stream outputStream, string password);
        void ZipDirectory(string directory, string archiveFile, string password = null, IProgress<int> progress = null);
        void ZipFiles(IEnumerable<string> files, string archiveFilePath, string password = null);
    }
}
