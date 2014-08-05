using System.Collections.Generic;
using WB.Core.SharedKernels.SurveyManagement.Views.DataExport;

namespace WB.Core.SharedKernels.SurveyManagement.Services
{
    internal interface IDataFileExportService {
        void AddRecord(InterviewDataExportLevelView items, string filePath);
        void AddActionRecords(IEnumerable<InterviewActionExportView> actions, string filePath);
        void CreateHeader(HeaderStructureForLevel header, string filePath);
        void CreateHeaderForActionFile(string filePath);
        string GetInterviewExportedDataFileName(string levelName);
        string GetInterviewActionFileName();
    }
}