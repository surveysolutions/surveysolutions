using FluentMigrator;

namespace WB.Persistence.Headquarters.Migrations.ReadSide
{
    [Migration(201903291300)]
    public class M201903291300_AddMissingIndexForCumulativeChartReport : Migration
    {
        public override void Up()
        {
            Create.Index("cumulativereportstatuschanges_interviewid_idx").OnTable("cumulativereportstatuschanges")
                .OnColumn("interviewid").Ascending();
        }

        public override void Down()
        {

        }
    }
}
