using FluentMigrator;

namespace WB.Persistence.Headquarters.Migrations.ReadSide
{
    [Migration(201709281806)]
    public class M201709281806_RemoveMapReportPointsTable : Migration
    {
        public override void Up()
        {
            Delete.Table("mapreportpoints");
        }

        public override void Down()
        {
        }
    }
}