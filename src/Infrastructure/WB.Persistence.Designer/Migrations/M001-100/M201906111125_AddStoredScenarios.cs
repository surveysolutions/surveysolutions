using FluentMigrator;

namespace WB.UI.Designer.Migrations.PlainStore
{
    [Migration(201906111125, "Added stored scenarios", BreakingChange = false)]
    public class M201906111125_AddStoredScenarios : AutoReversingMigration
    {
        public override void Up()
        {
            Create.Table("stored_scenarios")
                .WithColumn("id").AsInt32().Identity().PrimaryKey()
                .WithColumn("questionnaire_id").AsGuid().NotNullable()
                .WithColumn("steps").AsString()
                .WithColumn("title").AsString();
        }
    }
}
