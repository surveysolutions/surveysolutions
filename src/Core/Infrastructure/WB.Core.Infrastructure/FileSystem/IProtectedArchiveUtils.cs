using System;
using System.Collections.Generic;

namespace WB.Core.Infrastructure.FileSystem
{
    public interface IProtectedArchiveUtils
    {
        void ZipDirectory(string directory, string archiveFile, string password = null, IProgress<int> progress = null);
        void ZipFiles(IEnumerable<string> files, string archiveFilePath, string password = null);
    }
}
