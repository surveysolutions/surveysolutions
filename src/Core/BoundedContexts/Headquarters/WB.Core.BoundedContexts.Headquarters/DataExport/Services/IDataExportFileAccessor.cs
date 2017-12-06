using System;
using System.Collections.Generic;

namespace WB.Core.BoundedContexts.Headquarters.DataExport.Services
{
    public interface IDataExportFileAccessor
    {
        void RecreateExportArchive(string folderForDataExport, string archiveFilePath, IProgress<int> progress = null);
        void RecreateExportArchive(IEnumerable<string> filesToArchive, string archiveFilePath);
    }
}