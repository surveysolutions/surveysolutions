using FluentMigrator;

namespace WB.Persistence.Headquarters.Migrations.ReadSide
{
    [Migration(201805121700)]
    public class M201805121700_Add_categorical_pivot : Migration
    {
        public override void Up()
        {
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
            Execute.Sql(@"DROP FUNCTION readside.get_report_categorical_pivot(uuid,text,uuid,uuid)");
        }
    }
}
