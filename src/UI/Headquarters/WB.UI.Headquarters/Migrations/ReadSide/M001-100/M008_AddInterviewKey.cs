using FluentMigrator;

namespace WB.UI.Headquarters.Migrations.ReadSide
{
    [Migration(8)]
    public class M008_AddInterviewKey : Migration
    {
        public override void Up()
        {
            Alter.Table("interviewsummaries").AddColumn("key")
                .AsString(12).Nullable().Unique("interviewsummaries_unique_key");
        }

        public override void Down()
        {
            Delete.Column("key").FromTable("interviewsummaries");
        }
    }
}