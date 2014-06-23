using WB.Core.SharedKernels.SurveyManagement.Views.DataExport;

namespace WB.Core.SharedKernels.SurveyManagement.Services
{
    internal interface IDataFileExportService {
        void AddRecord(InterviewDataExportLevelView items, string filePath);
        void AddActionRecord(InterviewActionExportView interviewActionExportView, string filePath);
        void CreateHeader(HeaderStructureForLevel header, string filePath);
        void CreateHeaderForActionFile(string filePath);
        string GetInterviewExportedDataFileName(string levelName);
        string GetInterviewActionFileName();
    }
}