using System;
using System.Linq;
using Main.Core.Entities.SubEntities;
using WB.Core.BoundedContexts.Headquarters.Views.Reposts.SurveyStatistics.Data;
using WB.Core.BoundedContexts.Headquarters.Views.Reposts.Views;
using WB.Core.SharedKernels.DataCollection.Aggregates;
using WB.Core.SharedKernels.DataCollection.ValueObjects.Interview;

namespace WB.Core.BoundedContexts.Headquarters.Views.Reposts.SurveyStatistics
{
    internal class SurveyStatisticsReport : ISurveyStatisticsReport
    {
        private readonly IInterviewReportDataRepository interviewReportDataRepository;

        public SurveyStatisticsReport(IInterviewReportDataRepository interviewReportDataRepository)
        {
            this.interviewReportDataRepository = interviewReportDataRepository;
        }

        public ReportView GetReport(IQuestionnaire questionnaire, SurveyStatisticsReportInputModel input)
        {
            var questionType = questionnaire.GetQuestionType(input.QuestionId);

            var statuses = input.Statuses == null
                ? Array.Empty<InterviewStatus>()
                : input.Statuses.Select(s => Enum.Parse<InterviewStatus>(s, true)).ToArray();

            ReportView reportView;
            if (questionType == QuestionType.Numeric)
            {
                var specialValuesData = this.interviewReportDataRepository.GetCategoricalReportData(
                    new GetCategoricalReportParams(
                        input.QuestionnaireId,
                        input.QuestionnaireVersion,
                        input.ShowTeamMembers,
                        input.QuestionId,
                        input.TeamLeadId,
                        input.ConditionalQuestionId,
                        input.Condition)
                    {
                        Statuses = statuses
                    }
                );

                var questionOptions = questionnaire.GetOptionsForQuestion(input.QuestionId, null, null, null).ToList();
                var categoricalData = new CategoricalReportViewBuilder(questionOptions, specialValuesData);

                var numericalData = this.interviewReportDataRepository.GetNumericalReportData(
                    input.QuestionnaireId, input.QuestionnaireVersion,
                    input.QuestionId,
                    input.TeamLeadId,
                    input.ShowTeamMembers,
                    input.MinAnswer ?? Int64.MinValue, input.MaxAnswer ?? Int64.MaxValue,
                    statuses);

                var numericReport = new NumericalReportViewBuilder(numericalData);
                var specialValuesReport = categoricalData.AsReportView();

                reportView = numericReport.Merge(specialValuesReport);
            }
            else if (input.Pivot && input.ConditionalQuestionId.HasValue)
            {
                var queryResult = this.interviewReportDataRepository.GetCategoricalPivotData(input.TeamLeadId,
                    input.QuestionnaireId, input.QuestionnaireVersion,
                    input.QuestionId, input.ConditionalQuestionId.Value, 
                    statuses);

                var report = new CategoricalPivotReportViewBuilder(questionnaire,
                    input.QuestionId,
                    input.ConditionalQuestionId.Value,
                    queryResult);

                reportView = report.AsReportView();
            }
            else
            {
                var queryResult = this.interviewReportDataRepository.GetCategoricalReportData(
                    new GetCategoricalReportParams(
                        input.QuestionnaireId,
                        input.QuestionnaireVersion,
                        input.ShowTeamMembers,
                        input.QuestionId,
                        input.TeamLeadId,
                        input.Condition != null ? input.ConditionalQuestionId : null,
                        input.Condition) { Statuses = statuses });

                var questionOptions = questionnaire.GetOptionsForQuestion(input.QuestionId, null, null, null).ToList();
                var report = new CategoricalReportViewBuilder(questionOptions, queryResult);
                reportView = report.AsReportView();
            }

            if (input.Columns != null)
            {
                reportView = reportView.SelectColumns(input.Columns);
            }

            return reportView
                .ApplyOrderAndPaging(input.Orders, input.Page, input.PageSize);
        }
    }
}
