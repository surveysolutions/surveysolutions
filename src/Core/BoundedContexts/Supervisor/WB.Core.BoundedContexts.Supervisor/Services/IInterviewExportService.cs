using WB.Core.BoundedContexts.Supervisor.Views.DataExport;

namespace WB.Core.BoundedContexts.Supervisor.Services
{
    internal interface IInterviewExportService {
        void AddRecord(InterviewDataExportLevelView items, string filePath);
        void CreateHeader(HeaderStructureForLevel header, string filePath);
        string GetInterviewExportedDataFileName(string levelName);
    }
}