using FluentMigrator;

namespace WB.Persistence.Headquarters.Migrations.Workspaces
{
    [Migration(2021_01_04_1300)]
    public class M202101041300_AddSystemLogTableToWorkspaces : AutoReversingMigration
    {
        public override void Up()
        {
            Create.Table("systemlog")
                .WithColumn("id").AsInt32().Identity().PrimaryKey()
                .WithColumn("type").AsInt32()
                .WithColumn("logdate").AsDateTime()
                .WithColumn("userid").AsGuid().Nullable()
                .WithColumn("username").AsString().Nullable()
                .WithColumn("log").AsString();
        }
    }
}
