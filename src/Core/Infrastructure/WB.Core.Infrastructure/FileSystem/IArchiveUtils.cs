using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace WB.Core.Infrastructure.FileSystem
{
    public interface IArchiveUtils
    {
        void ZipDirectory(string directory, string archiveFile);
        void Unzip(string archivedFile, string extractToFolder);
    }
}
