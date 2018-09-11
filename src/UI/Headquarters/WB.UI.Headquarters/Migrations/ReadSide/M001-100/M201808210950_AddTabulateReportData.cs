using FluentMigrator;

namespace WB.UI.Headquarters.Migrations.ReadSide
{
    [Migration(201808210950)]
    public class M201808210950_AddTabulateReportData : Migration
    {
        public override void Up()
        {
            // cannot drop materialized views while there is depended functions
            Execute.Sql(@"drop view readside.report_tabulate_data_view");
            Execute.Sql(@"DROP FUNCTION readside.get_report_categorical_pivot(uuid,text,uuid,uuid)");
            Execute.Sql(@"DROP FUNCTION readside.get_numerical_report(uuid,text,uuid,bool,int8,int8)");
            Execute.Sql(@"DROP FUNCTION readside.get_categorical_report(text,bool,bool,uuid,uuid,uuid,_int8)");

            // we don't need to refresh data
            Execute.Sql(@"DROP FUNCTION readside.refresh_report_data()");

            // dropping view
            Execute.Sql("DROP MATERIALIZED VIEW readside.report_tabulate_numerical");
            Execute.Sql("DROP MATERIALIZED VIEW readside.report_tabulate_data");

            Create.Table("report_tabulate_data").InSchema("readside")
                .WithColumn("interview_id").AsInt32().NotNullable()
                .WithColumn("entity_id").AsInt32().NotNullable()
                .WithColumn("rostervector").AsString().NotNullable()
                .WithColumn("answer").AsInt64().NotNullable();

            Create.Table("report_tabulate_numerical").InSchema("readside")
                .WithColumn("interview_id").AsInt32().NotNullable()
                .WithColumn("entity_id").AsInt32().NotNullable()
                .WithColumn("rostervector").AsString().NotNullable()
                .WithColumn("answer").AsInt64().NotNullable();

            Execute.Sql("create unique index if not exists report_tabulate_data_unq_idx ON readside.report_tabulate_data (entity_id, rostervector, interview_id, answer)");
            Execute.Sql("create index if not exists report_tabulate_data_entity_id_idx ON readside.report_tabulate_data (entity_id)");
            Execute.Sql("create unique index if not exists report_tabulate_numerical_unq_idx ON readside.report_tabulate_numerical (entity_id, rostervector, interview_id, answer)");

            FillWithInitialData();

            Execute.Sql(@"CREATE OR REPLACE FUNCTION readside.update_report_table_data(_interviewid int)
                RETURNS void
                LANGUAGE sql
                AS $function$

                delete from readside.report_tabulate_data where interview_id = _interviewid;
                delete from readside.report_tabulate_numerical where interview_id = _interviewid;

                INSERT INTO readside.report_tabulate_data
                   SELECT a.interviewid AS interview_id,
                     a.entity_id,
                     a.rostervector,
                     ans.ans AS answer
                   FROM ( SELECT qe.id AS entity_id, i.rostervector, i.interviewid,
                                CASE
                                    WHEN i.asintarray IS NULL THEN ARRAY[COALESCE(i.asint::bigint, i.asdouble::bigint)]
                                    ELSE i.asintarray::bigint[]
                                END AS answer
                          FROM readside.interviews i JOIN readside.questionnaire_entities qe ON i.entityid = qe.id
                          WHERE 
          	                interviewid = _interviewid and
          	                (i.asint IS NOT NULL OR i.asdouble IS NOT NULL OR i.asintarray IS NOT NULL) 
          	                AND i.isenabled 
          	                AND (qe.question_type = ANY (ARRAY[0, 3, 4])) 
          	                AND qe.linked_to_question_id IS NULL 
          	                AND qe.linked_to_roster_id IS NULL 
          	                AND qe.cascade_from_question_id IS NULL 
          	                AND qe.is_filtered_combobox = false) a,
                   LATERAL unnest(a.answer) ans(ans);

                INSERT INTO readside.report_tabulate_numerical
                SELECT rd.interview_id,
                    rd.entity_id,
                    rd.rostervector,
                    rd.answer
                  FROM readside.report_tabulate_data rd
                  JOIN readside.questionnaire_entities qe ON qe.id = rd.entity_id
                   -- make sure this is not a predefined answer, so that it can be calculated
                  left join readside.questionnaire_entities_answers qea on qea.entity_id = rd.entity_id and qea.value = rd.answer::text
                  WHERE rd.interview_id = _interviewid and qe.question_type = 4 and qea.value is null;
             $function$");

            RestoreFunctions();
        }

        private void FillWithInitialData()
        {
            // fill initial data
            Execute.Sql(@"INSERT INTO readside.report_tabulate_data
                   SELECT a.interviewid AS interview_id,
                     a.entity_id,
                     a.rostervector,
                     ans.ans AS answer
                   FROM ( SELECT qe.id AS entity_id, i.rostervector, i.interviewid,
                                CASE
                                    WHEN i.asintarray IS NULL THEN ARRAY[COALESCE(i.asint::bigint, i.asdouble::bigint)]
                                    ELSE i.asintarray::bigint[]
                                END AS answer
                          FROM readside.interviews i JOIN readside.questionnaire_entities qe ON i.entityid = qe.id
                          WHERE 
          	                (i.asint IS NOT NULL OR i.asdouble IS NOT NULL OR i.asintarray IS NOT NULL) 
          	                AND i.isenabled 
          	                AND (qe.question_type = ANY (ARRAY[0, 3, 4])) 
          	                AND qe.linked_to_question_id IS NULL 
          	                AND qe.linked_to_roster_id IS NULL 
          	                AND qe.cascade_from_question_id IS NULL 
          	                AND qe.is_filtered_combobox = false) a,
                   LATERAL unnest(a.answer) ans(ans);");
           
            Execute.Sql(@"INSERT INTO readside.report_tabulate_numerical
                SELECT rd.interview_id,
                    rd.entity_id,
                    rd.rostervector,
                    rd.answer
                  FROM readside.report_tabulate_data rd
                  JOIN readside.questionnaire_entities qe ON qe.id = rd.entity_id
                   -- make sure this is not a categorical answer, so that it can be calculated
                  left join readside.questionnaire_entities_answers qea on qea.entity_id = rd.entity_id and qea.value = rd.answer::text
                  WHERE qe.question_type = 4 and qea.value is null");
        }

        private void RestoreFunctions()
        {
            // return back categorical report function
            Execute.Sql(@"CREATE OR REPLACE FUNCTION readside.get_categorical_report(_questionnaireidentity text, detailed boolean, 
                    totals boolean, _teamleadid uuid, _variable uuid, _condition_var uuid, _condition bigint[])
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
                                select agg.teamleadname, agg.responsiblename, agg.answer, count(distinct interview_id)
                                from (
                                    select 
                                        case when totals then null else s.teamleadid end as teamleadid,
                                        case when totals then null else s.teamleadname end as teamleadname,
                                        case when detailed then s.responsiblename else null end as responsiblename, 
                                        v1.interview_id, v1.answer 
                                    from readside.report_tabulate_data v1
                                    join readside.report_tabulate_data v2  on v1.interview_id = v2.interview_id
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
                                select agg.teamleadname, agg.responsiblename, agg.answer, count(distinct interview_id)
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
                $function$");

            Execute.Sql(@"CREATE OR REPLACE FUNCTION readside.get_numerical_report(_entityid uuid, _questionnaireidentity text, 
                        _teamleadid uuid, detailed boolean, minanswer bigint, maxanswer bigint)
                 RETURNS TABLE(teamleadname text, responsiblename text, count bigint, 
                    avg numeric, median numeric, min bigint, max bigint, sum numeric, 
                    percentile_05 double precision, percentile_50 double precision, percentile_95 double precision)
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
                            $function$");

            Execute.Sql(@"create or replace view readside.report_tabulate_data_view
                 as 
                select rd.*, id.interviewid, qe.entityid, qe.questionnaireidentity, qe.question_type, s.teamleadid, s.teamleadname, s.responsibleid, s.responsiblename --qe.questionnaireidentity, s.teamleadname
                from readside.report_tabulate_data rd
                join readside.questionnaire_entities qe on qe.id = rd.entity_id
                join readside.interviews_id id on id.id = rd.interview_id
                join readside.interviewsummaries s on id.interviewid = s.interviewid");

            Execute.Sql(@"CREATE OR REPLACE FUNCTION readside.get_report_categorical_pivot(_teamleadid uuid, _questionnaireid text, _a uuid, _b uuid)
                 RETURNS TABLE(a bigint, b bigint, count bigint)
                 LANGUAGE sql
                AS $function$	
                  with
  	                vara as (select id from readside.questionnaire_entities qe where qe.questionnaireidentity = _questionnaireid and qe.entityid = _a),
                    varb as (select id from readside.questionnaire_entities qe where qe.questionnaireidentity = _questionnaireid and qe.entityid = _b),
  	                agg as (
		                select v1.interview_id, v1.answer as a1, v2.answer as a2 
		                from readside.report_tabulate_data v1
		                join readside.report_tabulate_data v2  on v1.interview_id = v2.interview_id
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
        }

        public override void Down()
        {

        }
    }
}
