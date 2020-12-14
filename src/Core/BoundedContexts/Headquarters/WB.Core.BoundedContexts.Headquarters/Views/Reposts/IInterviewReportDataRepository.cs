using System;
using System.Collections.Generic;
using WB.Core.BoundedContexts.Headquarters.Views.Reposts.SurveyStatistics.Data;
using WB.Core.SharedKernels.DataCollection.Implementation.Entities;
using WB.Core.SharedKernels.DataCollection.ValueObjects.Interview;

namespace WB.Core.BoundedContexts.Headquarters.Views.Reposts
{
    public interface IInterviewReportDataRepository
    {
        List<QuestionnaireItem> QuestionsForQuestionnaireWithData(string questionnaireId, long? version);
        List<QuestionnaireIdentity> QuestionnairesWithData(Guid? teamLeadId);
        List<GetCategoricalReportItem> GetCategoricalReportData(GetCategoricalReportParams @params);

        List<GetNumericalReportItem> GetNumericalReportData(string questionnaireId, long? version,
            Guid questionId,
            Guid? teamLeadId,
            bool detailedView, long minAnswer = Int64.MinValue, long maxAnswer = Int64.MaxValue,
            InterviewStatus[] interviewStatuses = null);

        List<GetReportCategoricalPivotReportItem> GetCategoricalPivotData(Guid? teamLeadId,
            string questionnaire,
            long? version,
            Guid variableA, Guid variableB, InterviewStatus[] interviewStatuses);
    }
}
