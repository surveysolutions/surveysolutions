using System;
using System.Collections.Generic;
using WB.Core.Infrastructure.ReadSide.Repository.Accessors;
using WB.Core.SharedKernels.SurveyManagement.Views.DataExport;

namespace WB.Core.SharedKernels.SurveyManagement.Services
{
    public interface IDataExportService : IReadSideRepositoryCleaner
    {
        string GetFilePathToExportedCompressedData(Guid questionnaireId, long version);
        string GetFilePathToExportedApprovedCompressedData(Guid questionnaireId, long version);
        string GetFilePathToExportedBinaryData(Guid questionnaireId, long version);
        void AddExportedDataByInterview(InterviewDataExportView interviewDataExportView);
        void AddInterviewAction(Guid questionnaireId, long questionnaireVersion, InterviewActionExportView action);
        void CreateExportStructureByTemplate(QuestionnaireExportStructure questionnaireExportStructure);
        void DeleteExportedDataForQuestionnaireVersion(Guid questionnaireId, long questionnaireVersion);
    }
}
