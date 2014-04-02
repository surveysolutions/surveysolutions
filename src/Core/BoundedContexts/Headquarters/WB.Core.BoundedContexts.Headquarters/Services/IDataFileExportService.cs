using WB.Core.BoundedContexts.Headquarters.Views.DataExport;

namespace WB.Core.BoundedContexts.Headquarters.Services
{
    internal interface IDataFileExportService {
        void AddRecord(InterviewDataExportLevelView items, string filePath);
        void CreateHeader(HeaderStructureForLevel header, string filePath);
        string GetInterviewExportedDataFileName(string levelName);
    }
}