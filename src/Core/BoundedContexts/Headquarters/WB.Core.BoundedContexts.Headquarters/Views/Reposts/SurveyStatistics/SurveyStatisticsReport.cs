using System;
using Main.Core.Entities.SubEntities;
using WB.Core.BoundedContexts.Headquarters.Views.Reposts.SurveyStatistics.Data;
using WB.Core.BoundedContexts.Headquarters.Views.Reposts.Views;

namespace WB.Core.BoundedContexts.Headquarters.Views.Reposts.SurveyStatistics
{
    internal class SurveyStatisticsReport : ISurveyStatisticsReport
    {
        private readonly IInterviewReportDataRepository interviewReportDataRepository;

        public SurveyStatisticsReport(IInterviewReportDataRepository interviewReportDataRepository)
        {
            this.interviewReportDataRepository = interviewReportDataRepository;
        }

        public ReportView GetReport(SurveyStatisticsReportInputModel input)
        {
            var question = input.Question;
            ReportView reportView;
            if (question.QuestionType == QuestionType.Numeric)
            {
                var specialValuesData = this.interviewReportDataRepository.GetCategoricalReportData(
                    new GetCategoricalReportParams(
                        input.QuestionnaireIdentity.ToString(),
                        input.ShowTeamMembers,
                        input.Question.PublicKey,
                        input.TeamLeadId,
                        input.ConditionalQuestion?.PublicKey,
                        input.Condition,
                        input.ExcludeCategories)
                );

                var catergoricalData = new CategoricalReportViewBuilder(question.Answers, specialValuesData, input.ShowTeamLead, input.ShowTeamMembers);

                var numericalData = this.interviewReportDataRepository.GetNumericalReportData(
                    input.QuestionnaireIdentity,
                    input.Question.PublicKey,
                    input.TeamLeadId,
                    input.ShowTeamMembers,
                    input.MinAnswer ?? Int32.MinValue, input.MaxAnswer ?? Int32.MaxValue);

                var numericReport = new NumericalReportViewBuilder(numericalData, input.ShowTeamLead, input.ShowTeamMembers);
                var specialValuesReport = catergoricalData.AsReportView();

                reportView = numericReport.Merge(specialValuesReport);
            }
            else if (input.Pivot && input.ConditionalQuestion != null)
            {
                var queryResult = this.interviewReportDataRepository.GetCategoricalPivotData(input.TeamLeadId, input.QuestionnaireIdentity,
                    input.Question.PublicKey, input.ConditionalQuestion.PublicKey);

                var report = new CategoricalPivotReportViewBuilder(input.Question, input.ConditionalQuestion, queryResult);

                reportView = report.AsReportView();
            }
            else
            {
                var queryResult = this.interviewReportDataRepository.GetCategoricalReportData(
                    new GetCategoricalReportParams(
                        input.QuestionnaireIdentity.ToString(),
                        input.ShowTeamMembers,
                        input.Question.PublicKey,
                        input.TeamLeadId, 
                        input.Condition != null ? input.ConditionalQuestion?.PublicKey : null,
                        input.Condition,
                        input.ExcludeCategories));

                var report = new CategoricalReportViewBuilder(question.Answers, queryResult, input.ShowTeamLead, input.ShowTeamMembers);
                reportView = report.AsReportView();
            }

            return reportView
                .SelectColumns(input.Columns)
                .ApplyOrderAndPaging(input.Orders, input.Page, input.PageSize);
        }
    }
}
