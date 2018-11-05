using System;
using System.Collections.Generic;
using WB.Core.BoundedContexts.Headquarters.Views.Reposts.SurveyStatistics;
using WB.Core.BoundedContexts.Headquarters.Views.Reposts.SurveyStatistics.Data;
using WB.Core.SharedKernels.DataCollection.Implementation.Entities;

namespace WB.Core.BoundedContexts.Headquarters.Views.Reposts
{
    public interface IInterviewReportDataRepository
    {
        List<Guid> QuestionsForQuestionnaireWithData(QuestionnaireIdentity questionnaireIdentity);
        List<QuestionnaireIdentity> QuestionnairesWithData(Guid? teamLeadId);
        List<GetCategoricalReportItem> GetCategoricalReportData(GetCategoricalReportParams @params);

        List<GetNumericalReportItem> GetNumericalReportData(QuestionnaireIdentity questionnaireIdentity,
            Guid questionId,
            Guid? teamLeadId,
            bool detailedView, long minAnswer = Int64.MinValue, long maxAnswer = Int64.MaxValue);

        List<GetReportCategoricalPivotReportItem> GetCategoricalPivotData(Guid? teamLeadId,
            QuestionnaireIdentity questionnaire,
            Guid variableA, Guid variableB);
    }
}
