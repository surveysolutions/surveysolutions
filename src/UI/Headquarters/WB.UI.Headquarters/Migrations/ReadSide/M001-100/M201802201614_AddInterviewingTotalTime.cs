using FluentMigrator;

namespace WB.UI.Headquarters.Migrations.ReadSide
{
    [Migration(201802201614)]
    public class M201802201614_AddInterviewingTotalTime : Migration
    {
        public override void Up()
        {
            Create.Column("interviewingtotaltime").OnTable("interviewsummaries").AsInt64().Nullable();
            Create.Column("lastresumeeventutctimestamp").OnTable("interviewsummaries").AsDateTime().Nullable();
        }

        public override void Down()
        {
            Delete.Column("interviewingtotaltime").FromTable("interviewsummaries");
            Delete.Column("lastresumeeventutctimestamp").FromTable("interviewsummaries");
        }
    }
}