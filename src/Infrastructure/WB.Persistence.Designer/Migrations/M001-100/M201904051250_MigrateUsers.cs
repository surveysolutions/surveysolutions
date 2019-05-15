using FluentMigrator;

namespace WB.UI.Designer.Migrations.PlainStore
{
    [Migration(201904051250)]
    public class M201904051250_MigrateUsers : Migration
    {
        public override void Up()
        {
            Execute.EmbeddedScript(@"WB.Persistence.Designer.Migrations.M001_100.M201904051250_MigrateUsers.sql");
        }

        public override void Down()
        {
        }
    }
}
