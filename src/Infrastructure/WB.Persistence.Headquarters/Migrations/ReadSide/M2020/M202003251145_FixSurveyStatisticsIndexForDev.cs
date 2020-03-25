using FluentMigrator;

namespace WB.Persistence.Headquarters.Migrations.ReadSide
{
    [Migration(2020_03_25_11_45)]
    public class M202003251145_FixSurveyStatisticsIndexForDev : Migration
    {
        public override void Up()
        {
            // this is revert of M202003201151_IndexOnSurveyStatistics for all dev/hq/rc envs that deployed old code
            // this code will do nothing for production instances
            Execute.Sql("CREATE INDEX IF NOT EXISTS idx_report_statistics_entityid on readside.report_statistics (entity_id)");
            Execute.Sql("CREATE INDEX IF NOT EXISTS idx_report_statistics_interviewid on readside.report_statistics (entity_id)");
            Execute.Sql("DROP INDEX IF EXISTS readside.report_statistics_interview_id_idx");
        }

        public override void Down()
        {

        }
    }
}
