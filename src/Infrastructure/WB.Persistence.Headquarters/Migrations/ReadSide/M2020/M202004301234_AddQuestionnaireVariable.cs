using FluentMigrator;

namespace WB.Persistence.Headquarters.Migrations.ReadSide
{
    [Migration(2020_04_30_12_34)]
    public class M202004301234_AddQuestionnaireVariable : Migration
    {
        public override void Up()
        {
            Create.Column("questionnaire_variable").OnTable("interviewsummaries")
                .AsString()
                .NotNullable().SetExistingRowsTo("");
            
            Execute.Sql(@"update readside.interviewsummaries s
            set questionnaire_variable = variable 
            from (select variable, questionnaireid, ""version"" from plainstore.questionnairebrowseitems) q
                where s.questionnaireid = q.questionnaireid  and questionnaireversion = q.""version""");

            Create.Index().OnTable("interviewsummaries").OnColumn("questionnaire_variable");
        }

        public override void Down()
        {
            Delete.Column("questionnaire_variable").FromTable("interviewsummaries");
        }
    }
}
