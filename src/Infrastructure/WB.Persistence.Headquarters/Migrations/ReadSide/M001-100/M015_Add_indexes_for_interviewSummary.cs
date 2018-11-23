using FluentMigrator;

namespace WB.Persistence.Headquarters.Migrations.ReadSide
{
    [Migration(15)]
    public class M015_Add_indexes_for_interviewSummary : Migration
    {
        public override void Up()
        {
            this.Create.Index("interviewsummaries_responsiblename")
                .OnTable("interviewsummaries")
                .OnColumn("responsiblename");

            this.Create.Index("interviewsummaries_teamleadname")
                .OnTable("interviewsummaries")
                .OnColumn("teamleadname");
        }

        public override void Down()
        {
            this.Delete.Index("interviewsummaries_responsiblename");
            this.Delete.Index("interviewsummaries_teamleadname");
        }
    }
}