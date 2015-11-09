using System;

namespace WB.Core.SharedKernels.SurveyManagement.Services.Export
{
    public interface IFilebasedExportedDataAccessor
    {
        string GetFolderPathOfFilesByQuestionnaire(Guid questionnaireId, long version);
        string GetFilePathToExportedDDIMetadata(Guid questionnaireId, long version);

        string GetArchiveFilePathForExportedTabularData(Guid questionnaireId, long version);
        string GetArchiveFilePathForExportedApprovedTabularData(Guid questionnaireId, long version);

        string GetFolderPathOfDataByQuestionnaire(Guid questionnaireId, long version);
    }
}
