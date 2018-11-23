using FluentMigrator;

namespace WB.Persistence.Headquarters.Migrations.PlainStore
{
    [Migration(201802201015)]
    public class M201802201015_AddInterviewKeyToPackage : Migration
    {
        public override void Up()
        {
            Create.Column("interviewkey").OnTable("brokeninterviewpackages").AsString(12).Nullable();
        }

        public override void Down()
        {
            Delete.Column("interviewkey").FromTable("brokeninterviewpackages");
        }
    }
}