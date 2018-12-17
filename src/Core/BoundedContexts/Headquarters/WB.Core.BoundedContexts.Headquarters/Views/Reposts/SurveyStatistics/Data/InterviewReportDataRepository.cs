using System;
using System.Collections.Generic;
using System.Linq;
using Dapper;
using WB.Core.SharedKernels.DataCollection.Implementation.Entities;
using WB.Infrastructure.Native.Storage.Postgre;

namespace WB.Core.BoundedContexts.Headquarters.Views.Reposts.SurveyStatistics.Data
{
    partial class InterviewReportDataRepository : IInterviewReportDataRepository
    {
        private readonly IUnitOfWork sessionProvider;
        
        public InterviewReportDataRepository(IUnitOfWork sessionProvider)
        {
            this.sessionProvider = sessionProvider;
        }

        public List<QuestionnaireItem> QuestionsForQuestionnaireWithData(string questionnaireId, long? version)
        {
            return this.sessionProvider.Session.Connection
                .Query<QuestionnaireItem>(@"with questionnaires as (
	                    select id, entityid, questionnaireidentity from readside.questionnaire_entities
	                    where questionnaireidentity like @Id)
                    select q.entityid as questionId, q.questionnaireidentity from questionnaires q
                    where exists (
	                    select 1 from readside.report_tabulate_data rd
	                    where rd.entity_id = q.id)",
                    new { Id = $"{questionnaireId}${(version == null ? "%" : version.ToString())}"})
                .ToList();
        }

        public List<QuestionnaireIdentity> QuestionnairesWithData(Guid? teamLeadId)
        {
            return this.sessionProvider.Session.Connection
                .Query<string>(@"with questionnaires as (
	                select distinct questionnaireidentity
	                from readside.interviewsummaries
	                where @TeamLeadId is null or @TeamLeadId = teamleadid)
                select q.questionnaireidentity
                from questionnaires q
                where exists (
	                select 1
	                from readside.report_tabulate_data rd
	                JOIN readside.questionnaire_entities qe ON qe.id = rd.entity_id
	                where qe.questionnaireidentity = q.questionnaireidentity)", new { teamLeadId })
                .Select(QuestionnaireIdentity.Parse).ToList();
        }

        public List<GetReportCategoricalPivotReportItem> GetCategoricalPivotData(Guid? teamLeadId,
            string questionnaireIdentity,
            long? version,
            Guid variableA, Guid variableB)
        {
            var connection = this.sessionProvider.Session.Connection;

            return connection.Query<GetReportCategoricalPivotReportItem>(@"select a as colvalue, b as rowvalue, count
                from readside.get_report_categorical_pivot(@teamLeadId, @questionnaire, @variableA, @variableB)", new
            {
                teamLeadId,
                questionnaire = $"{questionnaireIdentity}${version?.ToString() ?? "%"}",
                variableA,
                variableB
            }).ToList();
        }

        public List<GetCategoricalReportItem> GetCategoricalReportData(GetCategoricalReportParams @params)
        {
            var connection = this.sessionProvider.Session.Connection;

            const string SqlQuery = @"select teamleadname, responsiblename, answer, count
                    from readside.get_categorical_report(@questionnaireIdentity, @detailed, @totals, @teamLeadId, 
                        @variable, @conditionVariable, @condition)";

            var result = connection.Query<GetCategoricalReportItem>(SqlQuery, @params);
            var totals = connection.Query<GetCategoricalReportItem>(SqlQuery, @params.AsTotals());

            return result.Concat(totals).ToList();
        }
        
        public List<GetNumericalReportItem> GetNumericalReportData(
            string questionnaireId, long? version,
            Guid questionId, Guid? teamLeadId,
            bool detailedView, long minAnswer = Int64.MinValue, long maxAnswer = Int64.MaxValue)
        {
            var session = this.sessionProvider.Session;
            var result = session.Connection.Query<GetNumericalReportItem>(@"
                select teamleadname, responsiblename,
                    count, avg as average, median, min, max, sum,
                    percentile_05 as percentile05,
                    percentile_50 as percentile50,
                    percentile_95 as percentile95
                from readside.get_numerical_report(@questionId, @questionnaireIdentity, @TeamLeadId, 
                    @detailedView, @minAnswer, @maxAnswer)",
                new
                {
                    questionnaireIdentity = $"{questionnaireId}${version?.ToString() ?? "%"}",
                    questionId,
                    teamLeadId,
                    detailedView,
                    minAnswer,
                    maxAnswer
                }).ToList();

            return result;
        }

    }
}
