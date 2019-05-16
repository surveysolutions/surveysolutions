using FluentMigrator;

namespace WB.Persistence.Headquarters.Migrations.PlainStore
{
    [Migration(201905161000)]
    public class M201905161000_CreateExceptionalTables : Migration
    {
        public override void Up()
        {
           Execute.EmbeddedScript("WB.Persistence.Headquarters.Migrations.PlainStore.M2019.ExceptionalTables.sql");
        }

        public override void Down()
        {
        }
    }
}
