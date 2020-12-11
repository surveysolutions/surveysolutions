using FluentMigrator;

namespace WB.Persistence.Headquarters.Migrations.Workspace
{
    [Migration(2020_11_20_14_21)]
    public class M202011201421_InitSingleWorkspace : ForwardOnlyMigration
    {
        public override void Up()
        {
            Execute.EmbeddedScript("WB.Persistence.Headquarters.Migrations.Workspace.Init.sql");
        }
    }
}
