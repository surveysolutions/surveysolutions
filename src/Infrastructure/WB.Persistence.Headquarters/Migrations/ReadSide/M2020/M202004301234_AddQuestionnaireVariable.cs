using FluentMigrator;

namespace WB.Persistence.Headquarters.Migrations.ReadSide
{
    [Migration(2020_04_30_12_34)]
    public class M202004301234_AddQuestionnaireVariable : Migration
    {
        public override void Up()
        {
            // this migration were merged to M202004101624_ExtendInterviewSummaryWithLowerCaseValuesAndVariables
            // code below is only to support dev environment databases. 
            if (!Schema.Table("interviewsummaries").Column("questionnaire_variable").Exists())
            {
                Create.Column("questionnaire_variable").OnTable("interviewsummaries")
                    .AsString().Nullable();

                if (Schema.Schema("plainstore").Table("questionnairebrowseitems").Exists())
                {
                    Execute.Sql(@"update readside.interviewsummaries s
                        set questionnaire_variable = coalesce(variable, q.questionnaireid::text) 
                        from (select variable, questionnaireid, ""version"" from plainstore.questionnairebrowseitems) q
                            where s.questionnaireid = q.questionnaireid  and questionnaireversion = q.""version""");
                }

                Execute.Sql(
                    "ALTER TABLE readside.interviewsummaries ALTER COLUMN questionnaire_variable SET NOT NULL;");
                Execute.Sql(
                    "ALTER TABLE readside.interviewsummaries ALTER COLUMN questionnaire_variable SET DEFAULT false;");

                Create.Index().OnTable("interviewsummaries").OnColumn("questionnaire_variable");
            }
        }

        public override void Down()
        {
            Delete.Column("questionnaire_variable").FromTable("interviewsummaries");
        }
    }
}
