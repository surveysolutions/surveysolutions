using System.ComponentModel;
using FluentMigrator;

namespace WB.Persistence.Headquarters.Migrations.ReadSide
{
    [Migration(201804161242)]
    [Localizable(false)]
    public class M201804161242_Add_materialized_view_report_tabulate_data_view : Migration
    {
        public override void Up()
        {
            Execute.Sql(@"CREATE MATERIALIZED VIEW IF NOT EXISTS readside.report_tabulate_data 
                as
                select a.interviewid as interview_id, a.entity_id, a.rostervector, ans as answer
                from (
                 select qe.id as entity_id, i.rostervector, i.interviewid, 
                    case when i.asintarray is null 
                        then array[coalesce(i.asint, i.asdouble::bigint)]
                        else i.asintarray
                    end answer
                 from readside.interviews i
                 inner join readside.questionnaire_entities qe  on i.entityid = qe.id
                 where (i.asint is not null or i.asdouble is not null or i.asintarray is not null)
                    and i.isenabled
                    and qe.question_type in (0, 3, 4) -- SingleQuestion, MultyOptionQuestion, numeric
                    and qe.linked_to_question_id    is null
                    and qe.linked_to_roster_id      is null
                    and qe.cascade_from_question_id is null
                    and qe.is_filtered_combobox   = false
                    ) a, unnest(a.answer) ans
                WITH NO DATA");

            Execute.Sql("create unique index if not exists report_tabulate_data_unq_idx ON readside.report_tabulate_data (entity_id, rostervector, interview_id, answer)");
            Execute.Sql("create index if not exists report_tabulate_data_entity_id_idx ON readside.report_tabulate_data (entity_id)");

            Execute.Sql(@"create or replace function readside.refresh_report_data() 
                returns void 
                as $$ begin
                    refresh materialized view concurrently readside.report_tabulate_data;
                end; $$ language plpgsql;");
        }

        public override void Down()
        {
            Execute.Sql(@"drop function readside.refresh_tabulate_data");
            Execute.Sql(@"drop materialized view readside.report_tabulate_data");
        }
    }
}
