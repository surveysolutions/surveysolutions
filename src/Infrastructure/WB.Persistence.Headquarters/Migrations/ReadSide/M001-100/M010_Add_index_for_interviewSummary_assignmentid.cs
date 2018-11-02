using FluentMigrator;

namespace WB.Persistence.Headquarters.Migrations.ReadSide
{
    [Migration(10)]
    public class M010_Add_index_for_interviewSummary_assignmentid : Migration
    {
        public override void Up()
        {
            this.Create.Index("interviewsummaries_assignmentid")
                .OnTable("interviewsummaries")
                .OnColumn("assignmentid");
        }

        public override void Down()
        {
            this.Delete.Index("interviewsummaries_assignmentid");
        }
    }
}