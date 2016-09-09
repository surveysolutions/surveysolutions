using FluentMigrator;

namespace WB.UI.Headquarters.Migrations.PlainStore
{
    [Migration(5)]
    public class M005_QuestionnaireRosterStructures : Migration
    {
        public override void Up()
        {
            Delete.Table("questionnairerosterstructures");
            Delete.Table("referenceinfoforlinkedquestions");

        }

        public override void Down()
        {
            Create.Table("questionnairerosterstructures")
                  .WithColumn("id").AsString().PrimaryKey()
                  .WithColumn("value").AsCustom("json").NotNullable();

            Create.Table("referenceinfoforlinkedquestions")
                  .WithColumn("id").AsString().PrimaryKey()
                  .WithColumn("value").AsCustom("json").NotNullable();
        }
    }
}