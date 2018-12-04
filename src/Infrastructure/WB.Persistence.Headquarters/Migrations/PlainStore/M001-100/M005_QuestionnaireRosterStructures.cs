using FluentMigrator;

namespace WB.Persistence.Headquarters.Migrations.PlainStore
{
    [Migration(5)]
    public class M005_QuestionnaireRosterStructures : Migration
    {
        public override void Up()
        {
            if (Schema.Table("questionnairerosterstructures").Exists())
            {
                Delete.Table("questionnairerosterstructures");
            }

            if (Schema.Table("referenceinfoforlinkedquestions").Exists())
            {
                Delete.Table("referenceinfoforlinkedquestions");
            }
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