using FluentMigrator;

namespace WB.Persistence.Headquarters.Migrations.ReadSide
{
    [Migration(201811291750)]
    public class M201811291750_FixStatisticsReports : Migration
    {
        public override void Up()
        {
            Execute.Sql(@"
CREATE OR REPLACE FUNCTION readside.get_report_categorical_pivot(_teamleadid uuid, _questionnaireid text, _a uuid, _b uuid)
 RETURNS TABLE(a bigint, b bigint, count bigint)
 LANGUAGE sql
AS $function$	
  with
  	vara as (select id from readside.questionnaire_entities qe where qe.questionnaireidentity = _questionnaireid and qe.entityid = _a),
    varb as (select id from readside.questionnaire_entities qe where qe.questionnaireidentity = _questionnaireid and qe.entityid = _b),
  	agg as (
		select v1.interview_id, v1.answer as a1, v2.answer as a2 
		from readside.report_tabulate_data v1
		join readside.report_tabulate_data v2  on v1.interview_id = v2.interview_id and v1.rostervector = v2.rostervector
		join readside.interviews_id id on id.id = v1.interview_id
		join readside.interviewsummaries s on s.interviewid = id.interviewid
		where 
	 		v1.entity_id in (select id from vara) 
	 		and v2.entity_id in (select id from varb)
			and (_teamleadid is null or _teamleadid = s.teamleadid)
	)
	select  a1, a2, count(distinct interview_id)
	from agg
	group by 1, 2
	order by 1, 2
$function$;");

            Execute.Sql(@"
CREATE OR REPLACE FUNCTION readside.get_categorical_report(_questionnaireidentity text, detailed boolean, totals boolean, _teamleadid uuid, _variable uuid, _condition_var uuid, _condition bigint[])
 RETURNS TABLE(teamleadname text, responsiblename text, answer bigint, count bigint)
 LANGUAGE plpgsql
AS $function$
    declare
        lookupVariable int;
        condVariable int;
    begin
        select id into lookupVariable from readside.questionnaire_entities where questionnaireidentity = _questionnaireidentity and entityid = _variable;
        select id into condVariable   from readside.questionnaire_entities where questionnaireidentity = _questionnaireidentity and entityid = _condition_var;
    
        -- join on self is costly, let's do not do this if don't needed
           if _condition_var is not null
           then
               RETURN QUERY
                select agg.teamleadname, agg.responsiblename, agg.answer, count(interview_id)
                from (
                    select 
                        case when totals then null else s.teamleadid end as teamleadid,
                        case when totals then null else s.teamleadname end as teamleadname,
                        case when detailed then s.responsiblename else null end as responsiblename, 
                        v1.interview_id, v1.answer 
                    from readside.report_tabulate_data v1
                    join readside.report_tabulate_data v2  on v1.interview_id = v2.interview_id  and v1.rostervector = v2.rostervector
                    join readside.interviews_id id on id.id = v1.interview_id
                    join readside.interviewsummaries s on s.interviewid = id.interviewid
                    join readside.questionnaire_entities_answers qea on qea.value::bigint = v1.answer and qea.entity_id = v1.entity_id
                    where 
                        (_teamleadid is null or s.teamleadid = _teamleadid) and
                        v1.entity_id = lookupVariable and
                    
                        -- filter by condition variable
                        v2.entity_id = condVariable and array[v2.answer] <@ _condition
                ) as agg
                group by 1, 2, 3
                order by 1, 2 ,3;
           else
               RETURN QUERY
                select agg.teamleadname, agg.responsiblename, agg.answer, count(interview_id)
                from (
                    select 
                        case when totals then null else s.teamleadid   end as teamleadid,
                        case when totals then null else s.teamleadname end as teamleadname,
                        case when detailed  then s.responsiblename else null end as responsiblename, 
                        v1.interview_id, v1.answer 
                    from readside.report_tabulate_data v1
                    join readside.interviews_id id on id.id = v1.interview_id
                    join readside.interviewsummaries s on s.interviewid = id.interviewid
                    join readside.questionnaire_entities_answers qea on qea.value::bigint = v1.answer and qea.entity_id = v1.entity_id
                    where 
                        (_teamleadid is null or s.teamleadid = _teamleadid) and
                        v1.entity_id = lookupVariable
                ) as agg
                group by 1, 2, 3
                order by 1, 2 ,3;
           end if;
    END;
$function$;");
        }

        public override void Down()
        {

        }
    }
}