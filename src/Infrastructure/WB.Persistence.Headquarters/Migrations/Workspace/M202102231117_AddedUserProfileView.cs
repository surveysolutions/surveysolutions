using System;
using FluentMigrator;
using WB.Infrastructure.Native.Workspaces;

namespace WB.Persistence.Headquarters.Migrations.Workspace
{
    [Migration(2021_02_23_11_17)]
    public class M202102231117_AddedUserProfileView : Migration
    {
        private readonly WorkspaceContext workspaceContext;

        public M202102231117_AddedUserProfileView(WorkspaceContext workspaceContext)
        {
            this.workspaceContext = workspaceContext;
        }

        public override void Up()
        {
            var schemaName = workspaceContext.SchemaName;
            var name = workspaceContext.Name;
            
            this.Execute.Sql(
            $"CREATE VIEW {schemaName}.user_profiles AS " + 
                "SELECT up.\"Id\", up.\"DeviceId\", ws.\"SupervisorId\", up.\"DeviceAppVersion\", up.\"DeviceAppBuildVersion\", up.\"DeviceRegistrationDate\", up.\"StorageFreeInBytes\" " +
                "FROM users.userprofiles up " +
                $"LEFT JOIN workspaces.workspace_users ws ON up.\"Id\" = ws.user_id AND ws.workspace = {name} ");
        }

        public override void Down()
        {
            throw new NotImplementedException();
        }
    }
}
