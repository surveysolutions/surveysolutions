using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
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

            return connection.Query<GetReportCategoricalPivotReportItem>(@"with
                 vara as (select id from readside.questionnaire_entities qe where qe.questionnaireidentity like @questionnaire and qe.entityid = @variableA),
                 varb as (select id from readside.questionnaire_entities qe where qe.questionnaireidentity like @questionnaire and qe.entityid = @variableB),
                 agg as (
	                select v1.interview_id, v1.answer as a1, v2.answer as a2, s.questionnaireidentity
	                from readside.report_tabulate_data v1
	                join readside.report_tabulate_data v2 on v1.interview_id = v2.interview_id
		                and 
			                (v1.rostervector = v2.rostervector
				                or concat(v1.rostervector, '-') like coalesce(nullif(v2.rostervector, '') || '-%', '%')
				                or concat(v2.rostervector, '-') like coalesce(nullif(v1.rostervector, '') || '-%', '%')
			                )
	                join readside.interviews_id id on id.id = v1.interview_id
	                join readside.interviewsummaries s on s.interviewid = id.interviewid
	                where 
 		                v1.entity_id in (select id from vara) 
 		                and v2.entity_id in (select id from varb)
		                and (@teamLeadId is null or @teamLeadId = s.teamleadid)
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

        [SuppressMessage("ReSharper", "StringLiteralTypo")]
        public List<GetCategoricalReportItem> GetCategoricalReportData(GetCategoricalReportParams @params)
        {
            var connection = this.sessionProvider.Session.Connection;

            string IfWithCondition(string sql) => @params.ConditionVariable != null ? sql: string.Empty;
            string IfSupervisor(string sql) => @params.TeamLeadId != null ? sql : string.Empty;

            string SqlQuery = $@"with
	    lookupVariable as (select id from readside.questionnaire_entities where questionnaireidentity like @questionnaireidentity 
                and entityid = @variable)
        {IfWithCondition(
         @", condVariable as (select id from readside.questionnaire_entities where questionnaireidentity like @questionnaireidentity 
                and entityid = @ConditionVariable)")
        } 
        select agg.teamleadname, agg.responsiblename, agg.answer, count(interview_id)
        from (
            select 
                case when @totals then null else s.teamleadid end as teamleadid,
                case when @totals then null else s.teamleadname end as teamleadname,
                case when @detailed then s.responsiblename else null end as responsiblename, 
                v1.interview_id, v1.answer 
            from readside.report_tabulate_data v1
            {IfWithCondition(
           @"join readside.report_tabulate_data v2  on v1.interview_id = v2.interview_id and 
                (v1.rostervector = v2.rostervector
                    or concat(v1.rostervector, '-') like coalesce(nullif(v2.rostervector, '') || '-%', '%')
                    or concat(v2.rostervector, '-') like coalesce(nullif(v1.rostervector, '') || '-%', '%')
                )")
            }
            join readside.interviews_id id on id.id = v1.interview_id
            join readside.interviewsummaries s on s.interviewid = id.interviewid
            join readside.questionnaire_entities_answers qea on qea.value::bigint = v1.answer and qea.entity_id = v1.entity_id
            where  
                {IfSupervisor("(@teamleadid is null or s.teamleadid = @teamleadid) and")}
                v1.entity_id in (select id from lookupVariable)     
                {IfWithCondition($@"-- filter by condition variable 
                and v2.entity_id in (select id from condVariable) and array[v2.answer] <@ @condition")}
        ) as agg 
        group by 1, 2, 3 
        order by 1, 2 ,3;";

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
