using FluentMigrator;

namespace WB.Persistence.Headquarters.Migrations.ReadSide
{
    [Migration(201812171300)]
    public class M201812171300_DropStatisticsReportFunctions : Migration
    {
        public override void Up()
        {
            // those functions are now moved to code
            Execute.Sql("DROP FUNCTION readside.get_categorical_report(text,bool,bool,uuid,uuid,uuid,_int8);");
            Execute.Sql("DROP FUNCTION readside.get_numerical_report(uuid,text,uuid,bool,int8,int8);");
            Execute.Sql("DROP FUNCTION readside.get_report_categorical_pivot(uuid,text,uuid,uuid);");
        }

        public override void Down()
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
		join readside.report_tabulate_data v2 on v1.interview_id = v2.interview_id
			and 
				(v1.rostervector = v2.rostervector -- compare answers from same roster or same roster tree leaf
                    -- postfix '-' is needed to prevent match of rosters ['1', '11']
                    -- if rostervector is empty then postfix '-' is not needed
                    -- || - is concat
					or concat(v1.rostervector, '-') like coalesce(nullif(v2.rostervector, '') || '-%', '%')
					or concat(v2.rostervector, '-') like coalesce(nullif(v1.rostervector, '') || '-%', '%')
				)
		join readside.interviews_id id on id.id = v1.interview_id
		join readside.interviewsummaries s on s.interviewid = id.interviewid
		where 
	 		v1.entity_id in (select id from vara) 
	 		and v2.entity_id in (select id from varb)
			and (_teamleadid is null or _teamleadid = s.teamleadid)
	)
	select  a1, a2, count(interview_id)
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
                    join readside.report_tabulate_data v2 on v1.interview_id = v2.interview_id
			            and (v1.rostervector = v2.rostervector -- compare answers from same roster or same roster tree leaf
                            -- postfix '-' is needed to prevent match of rosters ['1', '11']
                            -- if rostervector is empty then postfix '-' is not needed
                            -- || - is concat
					        or concat(v1.rostervector, '-') like coalesce(nullif(v2.rostervector, '') || '-%', '%')
					        or concat(v2.rostervector, '-') like coalesce(nullif(v1.rostervector, '') || '-%', '%')
				        )
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

            Execute.Sql(@"CREATE OR REPLACE FUNCTION readside.get_numerical_report(_entityid uuid, _questionnaireidentity text, _teamleadid uuid, detailed boolean, minanswer bigint, maxanswer bigint)
 RETURNS TABLE(teamleadname text, responsiblename text, count bigint, avg numeric, median numeric, min bigint, max bigint, sum numeric, percentile_05 double precision, percentile_50 double precision, percentile_95 double precision)
 LANGUAGE plpgsql
AS $function$
                                BEGIN
                                     RETURN QUERY
                                        with countables as (
                                            select s.teamleadname,
                                                case when detailed then s.responsiblename else null end as responsiblename, 
                                                qe.entityid, qe.questionnaireidentity, rd.answer
                                            from readside.report_tabulate_numerical rd
                                            inner join readside.questionnaire_entities qe on rd.entity_id = qe.id
                                            inner join readside.interviews_id id on rd.interview_id = id.id
                                            inner join readside.interviewsummaries s on id.interviewid = s.interviewid
                                            where 
                                                qe.entityid = _entityid and
                                                (_teamleadid is null or s.teamleadid = _teamleadid) and
                                                qe.questionnaireidentity = _questionnaireidentity and
                                                answer >= minanswer and answer <= maxanswer
                                        )
                                        select c.teamleadname, c.responsiblename,
                                            count(c.answer) as count, avg(c.answer) as avg, readside.median(c.answer) as median, min(c.answer) as min, max(c.answer) as max, sum(c.answer) as sum, 
                                            percentile_cont(0.05) within group (order by c.answer asc) as percentile_05,
                                            percentile_cont(0.5) within group (order by c.answer asc) as percentile_50,
                                            percentile_cont(0.95) within group (order by c.answer asc) as percentile_95
                                        from countables c
                                        group by 1,2
                                        
                                        union all
                                        
                                        select null, null,
                                            count(c.answer) as count, avg(c.answer) as avg, readside.median(c.answer) as median, min(c.answer) as min, max(c.answer) as max, sum(c.answer) as sum, 
                                            percentile_cont(0.05) within group (order by c.answer asc) as percentile_05,
                                            percentile_cont(0.50) within group (order by c.answer asc) as percentile_50,
                                            percentile_cont(0.95) within group (order by c.answer asc) as percentile_95
                                        from countables c;
                                END;
                            $function$
;");
        }
    }
}
