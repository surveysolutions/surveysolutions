using FluentMigrator;

namespace WB.Persistence.Headquarters.Migrations.ReadSide
{
    [Migration(201903211800)]
    public class M201903211800_CleanupDbFromInterviewsTable : Migration
    {
        public override void Up()
        {
            Execute.Sql("DROP FUNCTION if exists readside.interview_update(uuid, " +
                        "readside.interviewidentity[], readside.interviewidentity[], " +
                        "readside.interviewidentity[], readside.interviewvalidation[], " +
                        "readside.interviewidentity[], readside.interviewanswer[])");

            Execute.Sql("DROP FUNCTION if exists readside.update_report_table_data(int4);");

            Execute.Sql("DROP VIEW if exists readside.interviews_view");
            Execute.Sql("DROP VIEW if exists readside.report_tabulate_data_view");
            Execute.Sql("DROP TABLE if exists readside.report_tabulate_data");
            Execute.Sql("DROP TABLE if exists readside.report_tabulate_numerical");
            Execute.Sql("DROP TABLE if exists readside.interviews");
            Execute.Sql("DROP TABLE if exists readside.interviews_id");
            Execute.Sql("DROP TABLE if exists readside.interviewdatas");
            
        }

        public override void Down()
        {
            
        }
    }
}
