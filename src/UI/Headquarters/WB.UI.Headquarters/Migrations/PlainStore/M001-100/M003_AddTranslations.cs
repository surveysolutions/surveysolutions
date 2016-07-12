using FluentMigrator;

namespace WB.UI.Headquarters.Migrations.PlainStore
{
    [Migration(3)]
    public class M003_AddTranslations : Migration
    {
        public override void Up()
        {
            Create.Table("translationinstances")
                  .WithColumn("id").AsInt32().PrimaryKey()
                  .WithColumn("questionnaireid").AsGuid().NotNullable()
                  .WithColumn("type").AsInt32().NotNullable()
                  .WithColumn("questionnaireversion").AsInt64().NotNullable()
                  .WithColumn("questionnaireentityid").AsGuid().NotNullable()
                  .WithColumn("translationindex").AsString().Nullable()
                  .WithColumn("language").AsString().NotNullable()
                  .WithColumn("value").AsString().NotNullable();

            Create.Index("translationinstances_questionnaire_indx").OnTable("translationinstances")
                .OnColumn("questionnaireid").Ascending()
                .OnColumn("questionnaireversion").Ascending()
                .OnColumn("language").Ascending();
        }

        public override void Down()
        {
            Delete.Table("translationinstances");
        }
    }
}