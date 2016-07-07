using FluentMigrator;

namespace WB.UI.Designer.Migrations.PlainStore
{
    [Migration(3)]
    public class M003_AddTranslationsTable : Migration
    {
        public override void Up()
        {
            Create.Table("translationinstances")
                .WithColumn("questionnaireid").AsGuid().NotNullable().PrimaryKey()
                .WithColumn("type").AsInt32().NotNullable().PrimaryKey()
                .WithColumn("questionnaireentityid").AsGuid().NotNullable().PrimaryKey()
                .WithColumn("translationindex").AsString().Nullable().PrimaryKey()
                .WithColumn("culture").AsString().NotNullable().PrimaryKey()
                .WithColumn("translation").AsString().NotNullable();

            Create.Index("translationinstances_questionnaire")
                .OnTable("translationinstances")
                .OnColumn("questionnaireid").Ascending()
                .OnColumn("culture").Ascending()
                .OnColumn("questionnaireentityid").Ascending();
        }

        public override void Down()
        {
            Delete.Table("translationinstances");
        }
    }
}