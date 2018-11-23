using FluentMigrator;

namespace WB.Persistence.Headquarters.Migrations.ReadSide
{
    [Migration(201802201614)]
    public class M201802201614_AddInterviewingTotalTime : Migration
    {
        public override void Up()
        {
            Create.Column("interviewduration").OnTable("interviewsummaries").AsInt64().Nullable();
            Create.Column("lastresumeeventutctimestamp").OnTable("interviewsummaries").AsDateTime().Nullable();
        }

        public override void Down()
        {
            Delete.Column("interviewduration").FromTable("interviewsummaries");
            Delete.Column("lastresumeeventutctimestamp").FromTable("interviewsummaries");
        }
    }
}