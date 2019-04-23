using FluentMigrator;

namespace WB.UI.Designer.Migrations.Public
{
    [Migration(201904221727)]
    public class M201904221727_AddErrorsTable : Migration
    {
        public override void Up()
        {
            Execute.EmbeddedScript(@"WB.Persistence.Designer.Migrations.Public.M201904221727_AddErrorsTable.sql");
        }

        public override void Down()
        {
        }
    }
}
