using System;

namespace WB.Core.SharedKernels.SurveyManagement.Services.Export
{
    public interface IFilebasedExportedDataAccessor
    {
        string GetFolderPathOfFilesByQuestionnaireForInterview(Guid questionnaireId, long version, Guid interviewId);
        string GetFolderPathOfDataByQuestionnaire(Guid questionnaireId, long version);
        string GetFolderPathOfFilesByQuestionnaire(Guid questionnaireId, long version);
        string GetFolderPathOfHistoryByQuestionnaire(Guid questionnaireId, long version);


        string GetFilePathToExportedCompressedData(Guid questionnaireId, long version, ExportDataType exportDataType);
        string GetFilePathToExportedApprovedCompressedData(Guid questionnaireId, long version, ExportDataType exportDataType);

        string GetFilePathToExportedBinaryData(Guid questionnaireId, long version);
        string GetFilePathToExportedCompressedHistoryData(Guid questionnaireId, long version);

        string CreateExportDataFolder(Guid questionnaireId, long version);
        string CreateExportFileFolder(Guid questionnaireId, long version);

        void CleanExportDataFolder();
        void CleanExportFileFolder();
        void CleanExportHistoryFolder();
    }
}
