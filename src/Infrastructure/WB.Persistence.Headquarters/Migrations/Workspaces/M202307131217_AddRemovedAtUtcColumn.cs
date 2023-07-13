using FluentMigrator;

namespace WB.Persistence.Headquarters.Migrations.Workspaces
{
    [Migration(2023_07_13_12_17)]
    public class M202307131217_AddRemovedAtUtcColumn : AutoReversingMigration
    {
        public override void Up()
        {
            Create.Column("removed_at_utc").OnTable("workspaces")
                .AsDateTime().Nullable();
        }
    }
}
