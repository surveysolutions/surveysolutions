using System.Data;
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

            Execute.Sql($@" DELETE FROM readside.interviewcommentedstatuses st WHERE NOT EXISTS (
                              SELECT 1 FROM readside.interviewsummaries isum WHERE st.InterviewId = isum.SummaryId
                            ); ");

            Create.ForeignKey("FK_InterviewSummary_InterviewCommentedStatuses")
                .FromTable("interviewcommentedstatuses").ForeignColumn("interviewid")
                .ToTable("interviewsummaries").PrimaryColumn("summaryid")
                .OnDelete(Rule.Cascade);
        }

        public override void Down()
        {
            // Table is going to be created on first use
        }
    }
}