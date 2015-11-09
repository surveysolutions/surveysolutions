using System;

namespace WB.Core.BoundedContexts.Headquarters.DataExport.Accessors
{
    public interface IFilebasedExportedDataAccessor
    {
        string GetFolderPathOfFilesByQuestionnaire(Guid questionnaireId, long version);
        string GetFilePathToExportedDDIMetadata(Guid questionnaireId, long version);

        string GetArchiveFilePathForExportedData(Guid questionnaireId, long version);
        string GetArchiveFilePathForExportedApprovedTabularData(Guid questionnaireId, long version);

        string GetFolderPathOfDataByQuestionnaire(Guid questionnaireId, long version);
    }
}
