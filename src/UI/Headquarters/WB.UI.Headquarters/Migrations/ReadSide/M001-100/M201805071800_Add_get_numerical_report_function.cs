using System.ComponentModel;
using FluentMigrator;

namespace WB.UI.Headquarters.Migrations.ReadSide
{
    [Migration(201805071800)]
    [Localizable(false)]
    public class M201805071800_Add_get_numerical_report_function : Migration
    {
        public override void Up()
        {
            Execute.Sql(@" -- https://wiki.postgresql.org/wiki/Aggregate_Median
                CREATE OR REPLACE FUNCTION readside._final_median(NUMERIC[])
                   RETURNS NUMERIC AS
                $$
                   SELECT AVG(val)
                   FROM (
                     SELECT val
                     FROM unnest($1) val
                     ORDER BY 1
                     LIMIT  2 - MOD(array_upper($1, 1), 2)
                     OFFSET CEIL(array_upper($1, 1) / 2.0) - 1
                   ) sub;
                $$
                LANGUAGE 'sql' IMMUTABLE;
                 
                CREATE AGGREGATE readside.median(NUMERIC) (
                  SFUNC=array_append,
                  STYPE=NUMERIC[],
                  FINALFUNC=readside._final_median,
                  INITCOND='{}'
                );");

            Execute.Sql(@"create materialized view readside.report_tabulate_numerical
                as
                 select rd.*
                    from readside.report_tabulate_data rd
                    inner join readside.questionnaire_entities qe on qe.id = rd.entity_id
                    where qe.question_type = 4 and 
                     answer::text not in (
                        select value from readside.questionnaire_entities_answers qea 
                        where qea.entity_id = rd.entity_id)
                 order by answer, entityid
                with no data;");

            Execute.Sql("create unique index if not exists report_tabulate_numerical_unq_idx ON readside.report_tabulate_numerical (entity_id, rostervector, interview_id, answer)");

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

            Execute.Sql(@"create or replace function readside.refresh_report_data() 
                returns void
                as $$ begin
                    refresh materialized view concurrently readside.report_tabulate_data;
                    refresh materialized view concurrently readside.report_tabulate_numerical;
                end; $$ language plpgsql;");
        }

        public override void Down()
        {
            Execute.Sql(@"drop function readside.get_numercial_report");
            Execute.Sql(@"DROP MATERIALIZED VIEW readside.report_tabulate_numerical");
            Execute.Sql(@"drop aggregate readside.median");
            Execute.Sql(@"drop function readside._final_median");
        }
    }
}
