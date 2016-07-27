using FluentMigrator;

namespace WB.UI.Designer.Migrations.PlainStore
{
    [Migration(3)]
    public class M003_AddTranslationsTable : Migration
    {
        public override void Up()
        {
            Create.Table("translationinstances")
                .WithColumn("id").AsGuid().PrimaryKey()
                .WithColumn("questionnaireid").AsGuid().NotNullable()
                .WithColumn("type").AsInt32().NotNullable()
                .WithColumn("questionnaireentityid").AsGuid().NotNullable()
                .WithColumn("translationindex").AsString().Nullable()
                .WithColumn("translationid").AsGuid().NotNullable()
                .WithColumn("value").AsString().NotNullable();

            Create.Index("translationinstances_questionnaire")
                .OnTable("translationinstances")
                .OnColumn("questionnaireid").Ascending()
                .OnColumn("translationid").Ascending()
                .OnColumn("questionnaireentityid").Ascending();
        }

        public override void Down()
        {
            Delete.Table("translationinstances");
        }
    }
}