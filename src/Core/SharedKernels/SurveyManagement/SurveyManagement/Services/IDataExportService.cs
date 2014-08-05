using System;
using System.Collections.Generic;
using WB.Core.Infrastructure.ReadSide.Repository.Accessors;
using WB.Core.SharedKernels.SurveyManagement.Views.DataExport;

namespace WB.Core.SharedKernels.SurveyManagement.Services
{
    public interface IDataExportService : IReadSideRepositoryCleaner
    {
        string GetFilePathToExportedCompressedData(Guid questionnaireId, long version);
        void AddExportedDataByInterview(InterviewDataExportView interviewDataExportView);
        void AddInterviewActions(Guid questionnaireId, long questionnaireVersion, IEnumerable<InterviewActionExportView> actions);
        void CreateExportedDataStructureByTemplate(QuestionnaireExportStructure questionnaireExportStructure);
    }
}
