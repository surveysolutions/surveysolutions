using FluentMigrator;

namespace WB.Persistence.Headquarters.Migrations.Workspaces
{
    [Migration(2020_11_25_15_14)]
    public class M202011251514_CreateUserToWorkspaceMap : ForwardOnlyMigration
    {
        public override void Up()
        {
            Create.Table("workspace_users")
                .WithColumn("workspace").AsString().NotNullable()
                .WithColumn("user_id").AsGuid();

            Create.ForeignKey().FromTable("workspace_users").ForeignColumn("workspace")
                .ToTable("workspaces").PrimaryColumn("name");
                
            Create.ForeignKey().FromTable("workspace_users").ForeignColumn("user_id")
                .ToTable("users").InSchema("users").PrimaryColumn("Id");

            Insert.IntoTable("workspaces").Row(new {name = "primary", display_name = "Default"});
            Execute.Sql(@"insert into workspaces.workspace_users 
                        select 'primary', u.""Id"" 
                        from users.users u 
                        inner join users.userroles ur on u.""Id"" = ur.""UserId"" 
                        where ur.""RoleId"" not in ('00000000000000000000000000000001')
                        ");
        }
    }
}
