using FluentMigrator;

namespace WB.Persistence.Headquarters.Migrations.ReadSide
{
    [Migration(202005191600)]
    public class M202005191600_AddedMissingIndexForCumulativeChart : Migration
    {
        public override void Up()
        {
            Execute.Sql("CREATE INDEX cumulativereportstatuschanges_interviewid_idx " +
                        "ON readside.cumulativereportstatuschanges (interviewid,changevalue,eventsequence DESC);");
        }

        public override void Down()
        {
        }
    }
}
