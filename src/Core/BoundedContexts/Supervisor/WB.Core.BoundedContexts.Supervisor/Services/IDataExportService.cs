using System;
using System.Collections.Generic;
using WB.Core.BoundedContexts.Supervisor.Views.DataExport;

namespace WB.Core.BoundedContexts.Supervisor.Services
{
    public interface IDataExportService
    {
        string GetFilePathToExportedCompressedData(Guid questionnaireId, long version, string type);
        Dictionary<Guid, string> GetLevelIdToDataFilePathMap(InterviewDataExportView interviewDataExportView);
        void CreateDataFolderForTemplate(QuestionnaireExportStructure questionnaireExportStructure);
    }
}
