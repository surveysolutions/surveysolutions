using System.Collections.Generic;
using System.IO;

namespace WB.Core.Infrastructure.FileSystem
{
    public interface IZipArchiveProtectionService
    {
        void ZipFiles(IEnumerable<string> files, string archiveFilePath, string password = null);
    }
}
