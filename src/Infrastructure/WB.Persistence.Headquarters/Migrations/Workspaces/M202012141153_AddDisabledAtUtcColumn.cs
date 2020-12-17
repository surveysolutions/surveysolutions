using FluentMigrator;

namespace WB.Persistence.Headquarters.Migrations.Workspaces
{
    [Migration(2020_12_14_11_53)]
    public class M202012141153_AddDisabledAtUtcColumn : AutoReversingMigration
    {
        public override void Up()
        {
            Create.Column("disabled_at_utc").OnTable("workspaces")
                .AsDateTime().Nullable();
        }
    }
}
