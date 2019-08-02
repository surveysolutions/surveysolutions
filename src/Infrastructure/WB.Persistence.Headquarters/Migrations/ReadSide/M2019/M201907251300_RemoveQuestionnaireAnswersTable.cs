using FluentMigrator;

namespace WB.Persistence.Headquarters.Migrations.ReadSide
{
    [Migration(201907251300)]
    public class M201907251300_Remove_QuestionnaireEntitiesAnswers_Table : Migration
    {
        public override void Up()
        {
            Delete.Table("questionnaire_entities_answers");
        }

        public override void Down()
        {

        }
    }
}
