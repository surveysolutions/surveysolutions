using WB.Core.SharedKernels.SurveyManagement.Views.DataExport;

namespace WB.Core.SharedKernels.SurveyManagement.Services
{
    internal interface IDataFileExportService {
        void AddRecord(InterviewDataExportLevelView items, string filePath);
        void CreateHeader(HeaderStructureForLevel header, string filePath);
        string GetInterviewExportedDataFileName(string levelName);
    }
}