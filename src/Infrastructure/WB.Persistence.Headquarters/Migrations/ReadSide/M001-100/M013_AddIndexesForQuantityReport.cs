using FluentMigrator;

namespace WB.Persistence.Headquarters.Migrations.ReadSide
{
    [Migration(13)]
    public class M013_AddIndexesForQuantityReport : Migration
    {
        public override void Up()
        {
            Create.Index("interviewcommentedstatuses_timestamp").OnTable("interviewcommentedstatuses")
                .OnColumn("timestamp");
        }

        public override void Down()
        {
            Delete.Index("interviewcommentedstatuses_timestamp");
        }
    }
}