using FluentMigrator;

namespace WB.Persistence.Headquarters.Migrations.ReadSide
{
    [Migration(2020_03_20_11_51)]
    public class M202003201151_IndexOnSurveyStatistics : Migration
    {
        public override void Up()
        {
            Execute.Sql("CREATE INDEX  IF NOT EXISTS report_statistics_interview_id_idx ON readside.report_statistics USING btree (interview_id, entity_id) ");
            Execute.Sql("DROP INDEX IF EXISTS readside.idx_report_statistics_entityid ");
            Execute.Sql("DROP INDEX IF EXISTS readside.idx_report_statistics_interviewid ");
        }

        public override void Down()
        {
        }
    }
}
