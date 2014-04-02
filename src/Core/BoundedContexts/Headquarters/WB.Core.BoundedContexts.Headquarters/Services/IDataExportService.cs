using System;
using WB.Core.BoundedContexts.Headquarters.Views.DataExport;
using WB.Core.Infrastructure.ReadSide.Repository.Accessors;

namespace WB.Core.BoundedContexts.Headquarters.Services
{
    public interface IDataExportService : IReadSideRepositoryCleaner
    {
        string GetFilePathToExportedCompressedData(Guid questionnaireId, long version);
        void AddExportedDataByInterview(InterviewDataExportView interviewDataExportView);
        void CreateExportedDataStructureByTemplate(QuestionnaireExportStructure questionnaireExportStructure);
    }
}
