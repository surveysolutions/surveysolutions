using System;
using System.Collections.Generic;
using FluentMigrator;
using Npgsql;
using WB.Infrastructure.Native.Workspaces;

namespace WB.Persistence.Headquarters.Migrations.Workspace
{
    [Migration(2021_02_23_11_17)]
    public class M202102231117_AddedUserProfileView : ForwardOnlyMigration
    {
        public override void Up()
        {
            if (!this.Schema.Schema("users").Exists() 
                || !this.Schema.Schema("workspaces").Exists()
                || !this.Schema.Schema("workspaces").Table("workspace_users").Exists())
                return;
            
            var connectionStringBuilder = new NpgsqlConnectionStringBuilder(ConnectionString);
            var workspaceName = connectionStringBuilder.SearchPath!.Remove(0, "ws_".Length);
            
            Execute.EmbeddedScript(
                @"WB.Persistence.Headquarters.Migrations.Workspace.M202102231117_AddedUserProfileView.sql",
                new Dictionary<string, string>()
                {
                    { "workspace", workspaceName } 
                });
        }
    }
}
