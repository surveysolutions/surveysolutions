using FluentMigrator;

namespace WB.Persistence.Headquarters.Migrations.MigrateToPrimaryWorkspace
{
    // exists here for fluent migrator to record that 0 migration is already executed
    [Migration(2020_11_20_14_21)]
    public class M202011201421_InitSingleWorkspace : ForwardOnlyMigration
    {
        public override void Up()
        {
        }
    }
}
