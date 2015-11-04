using System;

namespace WB.Core.SharedKernels.SurveyManagement.Services.Export
{
    public interface IFilebasedExportedDataAccessor
    {
        string GetFolderPathOfFilesByQuestionnaireForInterview(Guid questionnaireId, long version, Guid interviewId);
        string GetFolderPathOfDataByQuestionnaire(Guid questionnaireId, long version);
        string GetFolderPathOfFilesByQuestionnaire(Guid questionnaireId, long version);
        string GetFilePathToExportedApprovedCompressedData(Guid questionnaireId, long version, ExportDataType exportDataType);
        string GetFilePathToExportedBinaryData(Guid questionnaireId, long version);
        string GetFilePathToExportedDDIMetadata(Guid questionnaireId, long version);
        string CreateExportFileFolder(Guid questionnaireId, long version);
        void CleanExportFileFolder();
        void DeleteApprovedDataFolder(Guid questionnaireId, long version);


        string GetFolderPathToExportedTabularData(Guid questionnaireId, long version);
        void CleanExportedTabularDataFolder(Guid questionnaireId, long version);
        void CreateArchiveOfExportedTabularData(Guid questionnaireId, long version);
        string GetArchiveFilePathForExportedTabularData(Guid questionnaireId, long version);
    }
}
