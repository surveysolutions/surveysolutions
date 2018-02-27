using System;
using System.Collections.Generic;
using System.IO;
using WB.Core.Infrastructure.FileSystem;

namespace WB.Core.BoundedContexts.Headquarters.DataExport.Services
{
    public interface IDataExportFileAccessor
    {
        IZipArchive CreateExportArchive(Stream outputStream);
        void RecreateExportArchive(string folderForDataExport, string archiveFilePath, IProgress<int> progress = null);
        void RecreateExportArchive(IEnumerable<string> filesToArchive, string archiveFilePath);
    }
}