using FluentMigrator;

namespace WB.Persistence.Headquarters.Migrations.PlainStore
{
    [Migration(201805311125)]
    public class M201805311125_AddResponsibleIdIndexForAssignments : Migration
    {
        public override void Up()
        {
            Create.Index().OnTable("assignments").OnColumn("responsibleid");
        }

        public override void Down()
        {
            Delete.Index().OnTable("assignments").OnColumn("responsibleid");
        }
    }
}
