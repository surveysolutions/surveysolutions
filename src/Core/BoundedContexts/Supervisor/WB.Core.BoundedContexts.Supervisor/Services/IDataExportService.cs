using System;
using System.Collections.Generic;
using WB.Core.BoundedContexts.Supervisor.Views.DataExport;
using WB.Core.Infrastructure.ReadSide.Repository.Accessors;

namespace WB.Core.BoundedContexts.Supervisor.Services
{
    public interface IDataExportService : IReadSideRepositoryCleaner
    {
        string GetFilePathToExportedCompressedData(Guid questionnaireId, long version);
        void AddExportedDataByInterview(InterviewDataExportView interviewDataExportView);
        void CreateExportedDataStructureByTemplate(QuestionnaireExportStructure questionnaireExportStructure);
        void DeleteExportedData(Guid questionnaireId, long version);
    }
}
