using FluentMigrator;

namespace WB.Persistence.Headquarters.Migrations.PlainStore
{
    [Migration(22)]
    public class M022_AddQuestionnaireIdColumn_to_Assignmenst : Migration
    {
        const string assignments = "assignments";
        const string column = "questionnaire";

        public override void Up()
        {
            this.Create.Column(column)
                .OnTable(assignments)
                .AsString(255)
                .Nullable();

            this.Execute.Sql(@"UPDATE plainstore.assignments 
                SET questionnaire=concat(replace(questionnaireid::text, '-', ''), '$', questionnaireversion)");
        }

        public override void Down()
        {
            this.Delete.Column(column)
                .FromTable(assignments);
        }
    }
}