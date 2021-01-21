using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using Dapper;
using WB.Core.SharedKernels.DataCollection.Implementation.Entities;
using WB.Core.SharedKernels.DataCollection.ValueObjects.Interview;
using WB.Infrastructure.Native.Storage.Postgre;

namespace WB.Core.BoundedContexts.Headquarters.Views.Reposts.SurveyStatistics.Data
{
    [SuppressMessage("ReSharper", "StringLiteralTypo")]
    class InterviewReportDataRepository : IInterviewReportDataRepository
    {
        private readonly IUnitOfWork sessionProvider;

        public InterviewReportDataRepository(IUnitOfWork sessionProvider)
        {
            this.sessionProvider = sessionProvider;
        }

        public List<QuestionnaireItem> QuestionsForQuestionnaireWithData(string questionnaireId, long? version)
        {
            return this.sessionProvider.Session.Connection
                .Query<QuestionnaireItem>($@"with questionnaires as (
	                    select id, entityid, questionnaireidentity from questionnaire_entities
	                    where questionnaireidentity like @Id)
                    select q.entityid as questionId, q.questionnaireidentity from questionnaires q
                    where exists (
	                    select 1 from report_statistics rd
	                    where rd.entity_id = q.id)",
                    new { Id = $"{questionnaireId}${(version == null ? "%" : version.ToString())}"})
                .ToList();
        }

        public List<QuestionnaireIdentity> QuestionnairesWithData(Guid? teamLeadId)
        {
            return this.sessionProvider.Session.Connection
                .Query<string>($@"with questionnaires as (
	                select distinct questionnaireidentity
	                from interviewsummaries
	                where @TeamLeadId is null or @TeamLeadId = teamleadid)
                select q.questionnaireidentity
                from questionnaires q
                where exists (
	                select 1
	                from report_statistics rd
	                JOIN questionnaire_entities qe ON qe.id = rd.entity_id
	                where qe.questionnaireidentity = q.questionnaireidentity)", new { teamLeadId })
                .Select(QuestionnaireIdentity.Parse).ToList();
        }

        public List<GetReportCategoricalPivotReportItem> GetCategoricalPivotData(Guid? teamLeadId,
            string questionnaireIdentity,
            long? version,
            Guid variableA, Guid variableB, InterviewStatus[] interviewStatuses)
        {
            var connection = this.sessionProvider.Session.Connection;
            string statuses = GetStatusesWhereExpression(interviewStatuses);

            return connection.Query<GetReportCategoricalPivotReportItem>($@"with
                 vara as (select id from questionnaire_entities qe where qe.questionnaireidentity like @questionnaire and qe.entityid = @variableA),
                 varb as (select id from questionnaire_entities qe where qe.questionnaireidentity like @questionnaire and qe.entityid = @variableB),
                 rep_a as (select * from report_statistics_categorical where entity_id in (select id from vara)),
                 rep_b as (select * from report_statistics_categorical where entity_id in (select id from varb)),
                 agg as (
	                select v1.interview_id, v1.answer as a1, v2.answer as a2, s.questionnaireidentity
	                from rep_a v1
	                join rep_b v2 on v1.interview_id = v2.interview_id
		                and 
			                (v1.rostervector = v2.rostervector
				                or concat(v1.rostervector, '-') like coalesce(nullif(v2.rostervector, '') || '-%', '%')
				                or concat(v2.rostervector, '-') like coalesce(nullif(v1.rostervector, '') || '-%', '%')
			                )
	                join interviewsummaries s on s.id = v1.interview_id
	                where (@teamLeadId is null or @teamLeadId = s.teamleadid) {statuses}
                )
                select  a1 as colvalue, a2 as rowvalue, count(interview_id)
                from agg
                group by 1, 2
                order by 1, 2", new
            {
                teamLeadId,
                questionnaire = $"{questionnaireIdentity}${version?.ToString() ?? "%"}",
                variableA,
                variableB
            }).ToList();
        }

        private string GetStatusesWhereExpression(InterviewStatus[] statuses, string interviewSummariesAlias = "s")
        {
            return statuses != null && statuses.Any()
                ? $"and {interviewSummariesAlias}.status in ({string.Join(", ", statuses.Select(s => (int)s))})"
                : "";
        }

        public List<GetCategoricalReportItem> GetCategoricalReportData(GetCategoricalReportParams @params)
        {
            var connection = this.sessionProvider.Session.Connection;

            string IfWithCondition(string sql) => @params.ConditionVariable != null ? sql: string.Empty;
            string IfSupervisor(string sql) => @params.TeamLeadId != null ? sql : string.Empty;

            string statuses = GetStatusesWhereExpression(@params.Statuses);

            string SqlQuery = $@"with
	        lookupVariable as (select id from questionnaire_entities where questionnaireidentity like @questionnaireidentity 
                and entityid = @variable),
            rep_a as (select * from report_statistics_categorical where entity_id in (select id from lookupVariable))
            {IfWithCondition($@",
            condVariable as (select id from questionnaire_entities where questionnaireidentity like @questionnaireidentity 
                and entityid = @ConditionVariable),
            rep_b as (select * from report_statistics_categorical where entity_id in (select id from condVariable))
            ")}
            select agg.teamleadname, agg.responsiblename, agg.answer, count(interview_id)
            from (
                select 
                    case when @totals then null else s.teamleadid end as teamleadid,
                    case when @totals then null else s.teamleadname end as teamleadname,
                    case when @detailed then s.responsiblename else null end as responsiblename, 
                    v1.interview_id, v1.answer 
                from rep_a v1
                {IfWithCondition( // we don't want to self join on report_statistics_categorical if no conditions specified
               @"join rep_b v2  on v1.interview_id = v2.interview_id and 
                    (v1.rostervector = v2.rostervector
                        or concat(v1.rostervector, '-') like coalesce(nullif(v2.rostervector, '') || '-%', '%')
                        or concat(v2.rostervector, '-') like coalesce(nullif(v1.rostervector, '') || '-%', '%')
                    )")
                }
                join interviewsummaries s on s.id = v1.interview_id
                where true = true {statuses}
                    {IfSupervisor(" and (@teamleadid is null or s.teamleadid = @teamleadid) ")}                    
                    {IfWithCondition(" and array[v2.answer] && @condition")}
            ) as agg 
            group by 1, 2, 3 
            order by 1, 2 ,3;";

            var result = connection.Query<GetCategoricalReportItem>(SqlQuery, @params);
            var totals = connection.Query<GetCategoricalReportItem>(SqlQuery, @params.AsTotals());

            return result.Concat(totals).ToList();
        }
        
        public List<GetNumericalReportItem> GetNumericalReportData(string questionnaireId, long? version,
            Guid questionId, Guid? teamLeadId,
            bool detailedView, long minAnswer = Int64.MinValue, long maxAnswer = Int64.MaxValue,
            InterviewStatus[] interviewStatuses = null)
        {
            var session = this.sessionProvider.Session;
            string statuses = GetStatusesWhereExpression(interviewStatuses);

            var result = session.Connection.Query<GetNumericalReportItem>($@"
                with countables as (
                    select s.teamleadname,
                        case when @detailedView then s.responsiblename else null end as responsiblename, 
                        qe.entityid, qe.questionnaireidentity, rd.answer
                    from report_statistics_numeric rd
                    inner join questionnaire_entities qe on rd.entity_id = qe.id
                    inner join interviewsummaries s on s.id = rd.interview_id
                    where 
                        qe.entityid = @questionId and
                        (@teamleadid is null or s.teamleadid = @teamleadid) and
                        qe.questionnaireidentity like @questionnaireIdentity and
                        answer >= @minanswer and answer <= @maxanswer {statuses}
                )
                select c.teamleadname, c.responsiblename,
                    count(c.answer) as count, avg(c.answer) as average, 
                    median(c.answer) as median, 
                    min(c.answer) as min, max(c.answer) as max, 
                    sum(c.answer) as sum, 
                    percentile_cont(0.05) within group (order by c.answer asc) as percentile05,
                    percentile_cont(0.5) within group (order by c.answer asc) as percentile50,
                    percentile_cont(0.95) within group (order by c.answer asc) as percentile95
                from countables c
                group by 1,2

                union all

                select null, null,
                    count(c.answer) as count, 
                    avg(c.answer) as average, 
                    median(c.answer) as median, 
                    min(c.answer) as min, max(c.answer) as max, 
                    sum(c.answer) as sum, 
                    percentile_cont(0.05) within group (order by c.answer asc) as percentile05,
                    percentile_cont(0.50) within group (order by c.answer asc) as percentile50,
                    percentile_cont(0.95) within group (order by c.answer asc) as percentile95
                from countables c;",
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
