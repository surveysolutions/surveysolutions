using FluentMigrator;

namespace WB.UI.Headquarters.Migrations.PlainStore
{
    [Migration(201806151438)]
    public class M201806151438_AddQuestionnaireVariableToQuestionnaireBrowseItems : Migration
    {
        public override void Up()
        {
            Alter.Table("questionnairebrowseitems").AddColumn("variable").AsString().Nullable();
        }

        public override void Down()
        {
            Delete.Column("variable").FromTable("questionnairebrowseitems");
        }
    }
}
