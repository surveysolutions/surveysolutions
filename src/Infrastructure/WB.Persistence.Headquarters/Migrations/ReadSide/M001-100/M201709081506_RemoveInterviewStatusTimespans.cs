using System.Data;
using FluentMigrator;

namespace WB.Persistence.Headquarters.Migrations.ReadSide
{
    [Migration(201709081506)]
    public class M201709081506_RemoveInterviewStatusTimespans : Migration
    {
        public override void Up()
        {
            Delete.ForeignKey("fk_interviewstatustimespans_timespanbetweenstatuses")
                .OnTable("timespanbetweenstatuses");

            Delete.ForeignKey("fk_interviewstatustimespans_timespansbetweenstatuses")
                .OnTable("timespanbetweenstatuses");

            Delete.Table("interviewstatustimespans");

            Execute.Sql($@"DELETE FROM readside.timespanbetweenstatuses WHERE NOT EXISTS (
                  SELECT 1 FROM readside.interviewsummaries WHERE readside.timespanbetweenstatuses.interviewid = readside.interviewsummaries.summaryid
            ); ");

            Create.ForeignKey("FK_InterviewSummary_TimeSpansBetweenStatuses")
                .FromTable("timespanbetweenstatuses").ForeignColumn("interviewid")
                .ToTable("interviewsummaries").PrimaryColumn("summaryid")
                .OnDelete(Rule.Cascade);
        }

        public override void Down()
        {
        }
    }
}