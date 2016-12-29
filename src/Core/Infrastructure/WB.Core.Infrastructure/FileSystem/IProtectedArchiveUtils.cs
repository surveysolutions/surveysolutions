using System.Collections.Generic;
using System.IO;

namespace WB.Core.Infrastructure.FileSystem
{
    public interface IProtectedArchiveUtils
    {
        void ZipDirectory(string directory, string archiveFile, string password = null);
        void ZipFiles(IEnumerable<string> files, string archiveFilePath, string password = null);
    }
}
