using System.Data;
using FluentMigrator;

namespace WB.Persistence.Headquarters.Migrations.ReadSide
{
    [Migration(201903291730)]
    public class M201903291730_RestoreForeignKeys :Migration
    {
        public override void Up()
        {
            // recreating dropped by cascade foreign keys

            Create.ForeignKey("fk_interviewsummaries_answerstofeaturedquestions")
                .FromTable("answerstofeaturedquestions")
                .ForeignColumn("interviewsummaryid")
                .ToTable("interviewsummaries").PrimaryColumn("summaryid");

            Create.ForeignKey("FK_InterviewSummary_InterviewCommentedStatuses")
                .FromTable("interviewcommentedstatuses").ForeignColumn("interviewid")
                .ToTable("interviewsummaries").PrimaryColumn("summaryid")
                .OnDelete(Rule.Cascade);

            Create.ForeignKey("FK_InterviewSummary_TimeSpansBetweenStatuses")
                .FromTable("timespanbetweenstatuses").ForeignColumn("interviewid")
                .ToTable("interviewsummaries").PrimaryColumn("summaryid")
                .OnDelete(Rule.Cascade);

            Create.ForeignKey("fk_interviewsummary_interviewflag").FromTable("interviewflags")
                .ForeignColumn("interviewid").ToTable("interviewsummaries").PrimaryColumn("summaryid")
                .OnDelete(Rule.Cascade);
        }

        public override void Down()
        {
            throw new System.NotImplementedException();
        }
    }
}