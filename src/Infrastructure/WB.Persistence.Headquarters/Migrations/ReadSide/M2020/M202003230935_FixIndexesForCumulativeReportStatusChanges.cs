using FluentMigrator;

namespace WB.Persistence.Headquarters.Migrations.ReadSide
{
    [Migration(202003230935)]
    public class M202003230935_FixIndexesForCumulativeReportStatusChanges : Migration
    {
        public override void Up()
        {
            Execute.Sql(@"DROP INDEX readside.cumulativereportstatuschanges_date;
                DROP INDEX readside.cumulativereportstatuschanges_eventsequence_indx;
                DROP INDEX readside.cumulativereportstatuschanges_interviewid_idx;");
        }

        public override void Down()
        {

        }
    }
}
