using FluentMigrator;

namespace WB.Persistence.Headquarters.Migrations.Logs
{
    [Migration(201905171139)]
    public class M201905171139_AddErrorsTable : Migration
    {
        public override void Up()
        {
            Execute.EmbeddedScript(@"WB.Persistence.Headquarters.Migrations.Logs.M201904221727_AddErrorsTable.sql");
        }

        public override void Down()
        {
        }
    }
}
