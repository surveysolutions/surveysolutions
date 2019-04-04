using FluentMigrator;

namespace WB.Persistence.Headquarters.Migrations.ReadSide
{
    [Migration(201902011557)]
    public class M201902011557_FixNaNValueForDoubleInTabulateReport : Migration
    {
        public override void Up()
        {
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
                            (i.asint IS NOT NULL 
                                OR (i.asdouble IS NOT NULL and i.asdouble != 'NAN'::float8) 
                                OR i.asintarray IS NOT NULL) 
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
        }

        public override void Down()
        {

        }
    }
}
