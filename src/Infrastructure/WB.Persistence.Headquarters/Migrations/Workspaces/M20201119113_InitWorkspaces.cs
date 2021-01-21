using FluentMigrator;

namespace WB.Persistence.Headquarters.Migrations.Workspaces
{
    [Migration(2020_11_19_11_14)]
    public class M202011191114_InitWorkspaces : ForwardOnlyMigration
    {
        public override void Up()
        {
            Create.Table("workspaces")
                .WithColumn("name").AsString().PrimaryKey()
                .WithColumn("display_name").AsString().NotNullable();
        }
    }
}
