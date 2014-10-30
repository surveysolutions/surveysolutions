using System;

namespace WB.Core.SharedKernels.SurveyManagement.Services.Export
{
    public interface IFilebasedExportedDataAccessor
    {
        string GetFolderPathOfFilesByQuestionnaireForInterview(Guid questionnaireId, long version, Guid interviewId);
        string GetFolderPathOfDataByQuestionnaireOrThrow(Guid questionnaireId, long version);
        string GetFolderPathOfFilesByQuestionnaireOrThrow(Guid questionnaireId, long version);

        string GetFilePathToExportedCompressedData(Guid questionnaireId, long version);
        string GetFilePathToExportedApprovedCompressedData(Guid questionnaireId, long version);
        string GetFilePathToExportedBinaryData(Guid questionnaireId, long version);

        string CreateExportDataFolder(Guid questionnaireId, long version);
        string CreateExportFileFolder(Guid questionnaireId, long version);

        void CleanExportDataFolder();
        void CleanExportFileFolder();
    }
}
