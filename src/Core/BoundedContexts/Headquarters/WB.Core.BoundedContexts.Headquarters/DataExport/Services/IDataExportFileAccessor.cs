using System.Collections.Generic;

namespace WB.Core.BoundedContexts.Headquarters.DataExport.Services
{
    public interface IDataExportFileAccessor
    {
        void RecreateExportArchive(string folderForDataExport, string archiveFilePath);
        void RecreateExportArchive(IEnumerable<string> filesToArchive, string archiveFilePath);
    }
}