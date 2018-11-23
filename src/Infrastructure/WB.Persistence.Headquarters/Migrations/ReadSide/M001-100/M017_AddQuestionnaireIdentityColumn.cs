using System.ComponentModel;
using FluentMigrator;

namespace WB.Persistence.Headquarters.Migrations.ReadSide
{

    [Localizable(false)]
    [Migration(17)]
    public class M017_AddQuestionnaireIdentityColumn : Migration
    {
        private static readonly string InterviewSummariesTable = "interviewsummaries";
        private static readonly string QuestionnaireIdentityColumn = "questionnaireidentity";

        public override void Up()
        {
            Alter.Table(InterviewSummariesTable).AddColumn(QuestionnaireIdentityColumn).AsString().Nullable();

            Execute.Sql($@"UPDATE readside.{InterviewSummariesTable} 
                           SET {QuestionnaireIdentityColumn}=concat(replace(questionnaireid::text, '-', ''), '$', questionnaireversion)");

            Create.Index("interviewsummaries_questionnaire_identity_indx").OnTable(InterviewSummariesTable).OnColumn(QuestionnaireIdentityColumn);
        }

        public override void Down()
        {
            Delete.Column(QuestionnaireIdentityColumn).FromTable(InterviewSummariesTable);
        }
    }
}