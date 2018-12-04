using FluentMigrator;

namespace WB.Persistence.Headquarters.Migrations.ReadSide
{
    [Migration(16)]
    public class M016_AddQuestionnaireIdAndVersionIndexesToInterviewSummary : Migration
    {
        public override void Up()
        {
            Create.Index("interviewsummaries_questionnaire_id_indx").OnTable("interviewsummaries").OnColumn("questionnaireid");
            Create.Index("interviewsummaries_questionnaire_version_indx").OnTable("interviewsummaries").OnColumn("questionnaireversion");
        }

        public override void Down()
        {
            Delete.Index("interviewsummaries_questionnaire_id_indx");
            Delete.Index("interviewsummaries_questionnaire_version_indx");
        }
    }
}