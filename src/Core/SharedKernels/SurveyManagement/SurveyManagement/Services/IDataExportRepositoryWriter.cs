using System;
using System.Collections.Generic;
using WB.Core.Infrastructure.ReadSide;
using WB.Core.Infrastructure.ReadSide.Repository.Accessors;
using WB.Core.SharedKernels.SurveyManagement.Views.DataExport;

namespace WB.Core.SharedKernels.SurveyManagement.Services
{
    public interface IDataExportRepositoryWriter : IReadSideRepositoryCleaner, ICacheableRepositoryWriter
    {
        void AddOrUpdateExportedDataByInterviewWithAction(Guid interviewId,InterviewExportedAction action);
        void DeleteInterview(Guid interviewId);
        void CreateExportStructureByTemplate(QuestionnaireExportStructure questionnaireExportStructure);
        void DeleteExportedDataForQuestionnaireVersion(Guid questionnaireId, long questionnaireVersion);
    }
}
