using System.Data;
using FluentMigrator;

namespace WB.Persistence.Headquarters.Migrations.ReadSide
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
            Delete.ForeignKey("FK_InterviewSummary_InterviewCommentedStatuses")
                .OnTable("interviewcommentedstatuses");

            Create.Table("interviewstatuses")
                .WithColumn("id").AsString(255).PrimaryKey()
                .WithColumn("questionnaireid").AsGuid().Nullable()
                .WithColumn("questionnaireversion").AsInt64().Nullable();

            Create.ForeignKey("interviewcommentedstatuses")
                .FromTable("interviewcommentedstatuses").ForeignColumn("interviewid")
                .ToTable("interviewstatuses").PrimaryColumn("id");
        }
    }
}