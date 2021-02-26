using System;
using FluentMigrator;
using Npgsql;
using WB.Infrastructure.Native.Workspaces;

namespace WB.Persistence.Headquarters.Migrations.Workspace
{
    [Migration(2021_02_23_11_17)]
    public class M202102231117_AddedUserProfileView : Migration
    {
        public override void Up()
        {
            if (!this.Schema.Schema("users").Exists() 
                || !this.Schema.Schema("workspaces").Exists()
                || !this.Schema.Schema("workspaces").Table("workspace_users").Exists())
                return;
            
            var connectionStringBuilder = new NpgsqlConnectionStringBuilder(ConnectionString);
            var name = connectionStringBuilder.SearchPath!.Remove(0, "ws_".Length);
            
            this.Execute.Sql(
            $"CREATE VIEW user_profiles AS " + 
                "SELECT up.\"Id\", up.\"DeviceId\", ws.supervisor_id as \"SupervisorId\", up.\"DeviceAppVersion\", up.\"DeviceAppBuildVersion\", up.\"DeviceRegistrationDate\", up.\"StorageFreeInBytes\" " +
                "FROM users.userprofiles up " +
                $"INNER JOIN users.users uu ON uu.\"UserProfileId\" = up.\"Id\" " +
                $"LEFT JOIN workspaces.workspace_users ws ON uu.\"Id\" = ws.user_id AND ws.workspace = '{name}' ");
        }

        public override void Down()
        {
            throw new NotImplementedException();
        }
    }
}
