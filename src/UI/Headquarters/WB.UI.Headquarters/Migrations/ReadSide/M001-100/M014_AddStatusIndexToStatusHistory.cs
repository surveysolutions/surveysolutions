using FluentMigrator;

namespace WB.UI.Headquarters.Migrations.ReadSide
{
    [Migration(14)]
    public class M014_AddStatusIndexToStatusHistory : Migration
    {
        public override void Up()
        {
            Create.Index("interviewcommentedstatuses_status_indx").OnTable("interviewcommentedstatuses").OnColumn("status");
        }

        public override void Down()
        {
            Delete.Index("interviewcommentedstatuses_status_indx");
        }
    }
}
