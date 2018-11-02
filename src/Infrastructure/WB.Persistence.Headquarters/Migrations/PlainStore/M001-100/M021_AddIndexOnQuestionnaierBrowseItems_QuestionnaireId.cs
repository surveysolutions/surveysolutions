using FluentMigrator;

namespace WB.Persistence.Headquarters.Migrations.PlainStore
{
    [Migration(21)]
    public class M021_AddIndexOnQuestionnaierBrowseItems_QuestionnaireId : Migration
    {
        public override void Up()
        {
            Create.Index("idx_Questionnaire_id").OnTable("questionnairebrowseitems").OnColumn("questionnaireid");
        }

        public override void Down()
        {
            Delete.Index("idx_Questionnaire_id").OnTable("questionnairebrowseitems");
        }
    }
}