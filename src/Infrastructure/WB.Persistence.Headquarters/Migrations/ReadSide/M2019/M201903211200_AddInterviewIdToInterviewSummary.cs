using FluentMigrator;

namespace WB.Persistence.Headquarters.Migrations.ReadSide
{
    [Migration(201903211200)]
    public class M201903211200_AddInterviewIdToInterviewSummary : Migration
    {
        public override void Up()
        {
            Alter.Table("interviewsummaries")
                .AddColumn("id")
                .AsCustom("serial")
                .NotNullable();

            Create.UniqueConstraint("unq_interviewsummaries_id")
                .OnTable("interviewsummaries").Column("id");
        }

        public override void Down()
        {
            Delete.Column("id").FromTable("interviewsummaries");
        }
    }
}
