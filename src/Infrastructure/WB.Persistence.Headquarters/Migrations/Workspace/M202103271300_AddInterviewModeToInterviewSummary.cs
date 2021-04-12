using FluentMigrator;

namespace WB.Persistence.Headquarters.Migrations.Workspace
{
    [Migration(202103271300)]
    public class M202103271300_AddInterviewModeToInterviewSummary : AutoReversingMigration
    {
        enum InterviewMode
        {
            Unknown = 0,
            CAPI = 1,
            CAWI = 2
        }

        public override void Up()
        {
            Alter.Table("interviewsummaries")
                .AddColumn("interview_mode").AsInt32().NotNullable()
                .Indexed("ix_interview_summaries_mode")
                .SetExistingRowsTo((int)InterviewMode.Unknown);
        }
    }
}
