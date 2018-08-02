using FluentMigrator;

namespace WB.UI.Headquarters.Migrations.ReadSide
{
    [Migration(11)]
    public class M011_AddOriginalHumanIdToInterviewSummary : Migration
    {
        public override void Up()
        {
            Create.Column("clientkey").OnTable("interviewsummaries")
                .AsString(12).Nullable();

            Delete.Index("interviewsummaries_unique_key").OnTable("interviewsummaries");
            Create.Index("interviewsummaries_unique_key").OnTable("interviewsummaries")
                .OnColumn("key").Unique().OnColumn("clientkey").Ascending();
        }

        public override void Down()
        {
            Delete.Column("clientkey").FromTable("interviewsummaries");

            Delete.Index("interviewsummaries_unique_key");
            Create.Index("interviewsummaries_unique_key").OnTable("interviewsummaries").OnColumn("key").Unique();
        }
    }
}
