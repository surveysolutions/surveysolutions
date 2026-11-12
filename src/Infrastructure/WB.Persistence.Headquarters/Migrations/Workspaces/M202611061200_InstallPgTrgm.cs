using FluentMigrator;

namespace WB.Persistence.Headquarters.Migrations.Workspaces
{
    [Migration(202611061200)]
    public class M202611061200_InstallPgTrgm : ForwardOnlyMigration
    {
        public override void Up()
        {
            Execute.Sql("CREATE EXTENSION IF NOT EXISTS pg_trgm WITH SCHEMA public;");
        }
    }
}
