using System;

namespace WB.Core.SharedKernels.SurveyManagement.Services.Export
{
    public interface IFilebasedExportedDataAccessor
    {
        string GetFolderPathOfFilesByQuestionnaireForInterview(Guid questionnaireId, long version, Guid interviewId);
        string GetFolderPathOfFilesByQuestionnaire(Guid questionnaireId, long version);
        string GetFilePathToExportedBinaryData(Guid questionnaireId, long version);
        string GetFilePathToExportedDDIMetadata(Guid questionnaireId, long version);
        string CreateExportFileFolder(Guid questionnaireId, long version);
        void CleanExportFileFolder();

        string GetArchiveFilePathForExportedTabularData(Guid questionnaireId, long version);
        string GetArchiveFilePathForExportedApprovedTabularData(Guid questionnaireId, long version);

        string GetFolderPathOfDataByQuestionnaire(Guid questionnaireId, long version);
    }
}
