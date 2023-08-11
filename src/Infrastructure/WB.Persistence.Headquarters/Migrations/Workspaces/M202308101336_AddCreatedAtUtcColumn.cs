using FluentMigrator;

namespace WB.Persistence.Headquarters.Migrations.Workspaces
{
    [Migration(2023_08_10_13_36)]
    public class M202308101336_AddCreatedAtUtcColumn : AutoReversingMigration
    {
        public override void Up()
        {
            Create.Column("created_at_utc").OnTable("workspaces")
                .AsDateTime().Nullable();
        }
    }
}
