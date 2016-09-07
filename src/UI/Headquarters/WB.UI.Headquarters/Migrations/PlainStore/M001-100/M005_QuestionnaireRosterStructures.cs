using FluentMigrator;

namespace WB.UI.Headquarters.Migrations.PlainStore
{
    [Migration(5)]
    public class M005_QuestionnaireRosterStructures : Migration
    {
        public override void Up()
        {
            Delete.Table("questionnairerosterstructures");
        }

        public override void Down()
        {
            Create.Table("questionnairerosterstructures")
                  .WithColumn("id").AsInt32().PrimaryKey()
                  .WithColumn("value").AsCustom("json").NotNullable();
        }
    }
}