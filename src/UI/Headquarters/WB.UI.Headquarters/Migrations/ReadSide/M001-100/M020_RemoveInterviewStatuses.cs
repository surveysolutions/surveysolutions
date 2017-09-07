using FluentMigrator;

namespace WB.UI.Headquarters.Migrations.ReadSide
{
    [Migration(20)]
    public class M020_RemoveInterviewStatuses : Migration
    {
        public override void Up()
        {
            Delete.ForeignKey("interviewcommentedstatuses")
                .OnTable("interviewcommentedstatuses");

            Delete.Table("interviewstatuses");

            Create.ForeignKey("FK_InterviewSummary_InterviewCommentedStatuses")
                .FromTable("interviewcommentedstatuses").ForeignColumn("interviewid")
                .ToTable("interviewsummaries").PrimaryColumn("summaryid");
        }

        public override void Down()
        {
            // Table is going to be created on first use
        }
    }
}