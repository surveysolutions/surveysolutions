using System.Data;
using FluentMigrator;

namespace WB.Persistence.Headquarters.Migrations.Workspaces
{
    [Migration(2020_11_25_15_14)]
    public class M202011251514_CreateUserToWorkspaceMap : ForwardOnlyMigration
    {
        public override void Up()
        {
            Create.Table("workspace_users")
                .WithColumn("id").AsInt32().Identity().PrimaryKey()
                .WithColumn("workspace").AsString().NotNullable()
                .WithColumn("user_id").AsGuid();

            Create.ForeignKey().FromTable("workspace_users").ForeignColumn("workspace")
                .ToTable("workspaces").PrimaryColumn("name").OnDelete(Rule.Cascade);
                
            Create.ForeignKey().FromTable("workspace_users").ForeignColumn("user_id")
                .ToTable("users").InSchema("users").PrimaryColumn("Id").OnDelete(Rule.Cascade);

            Insert.IntoTable("workspaces")
                .Row(new {name = "primary", display_name = "Default Workspace"});

            Execute.Sql(@"insert into workspaces.workspace_users (workspace, user_id)
                        select 'primary' as workspace, u.""Id"" as user_id 
                        from users.users u 
                        inner join users.userroles ur on u.""Id"" = ur.""UserId"" 
                        where ur.""RoleId"" not in ('00000000000000000000000000000001')
                        ");

            Create.Index().OnTable("workspace_users").OnColumn("user_id").Unique().OnColumn("workspace").Unique();
        }
    }
}
