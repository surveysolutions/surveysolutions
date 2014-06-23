using System;
using WB.Core.Infrastructure.ReadSide.Repository.Accessors;
using WB.Core.SharedKernels.SurveyManagement.Views.DataExport;

namespace WB.Core.SharedKernels.SurveyManagement.Services
{
    public interface IDataExportService : IReadSideRepositoryCleaner
    {
        string GetFilePathToExportedCompressedData(Guid questionnaireId, long version);
        void AddExportedDataByInterview(InterviewDataExportView interviewDataExportView);
        void AddInterviewAction(InterviewActionExportView interviewActionExportView);
        void CreateExportedDataStructureByTemplate(QuestionnaireExportStructure questionnaireExportStructure);
    }
}
