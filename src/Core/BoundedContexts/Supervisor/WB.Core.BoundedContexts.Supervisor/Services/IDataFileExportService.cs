using WB.Core.BoundedContexts.Supervisor.Views.DataExport;

namespace WB.Core.BoundedContexts.Supervisor.Services
{
    internal interface IDataFileExportService {
        void AddRecord(InterviewDataExportLevelView items, string filePath);
        void CreateHeader(HeaderStructureForLevel header, string filePath);
        string GetInterviewExportedDataFileName(string levelName);
    }
}