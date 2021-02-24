using System.IO;
using FluentMigrator;
using Microsoft.Extensions.Configuration;
using WB.Core.SharedKernels.DataCollection;

namespace WB.Persistence.Headquarters.Migrations.Workspaces
{
    [Migration(2021_02_23_11_17)]
    public class M202102231117_AddedSupervisorIdColumnToWorkspaceUsers : Migration
    {
        public override void Up()
        {
            this.Create.Column("supervisor_id").OnTable("workspace_users")
                .AsGuid().Nullable();
            
            this.Execute.Sql(@"
                UPDATE workspaces.workspace_users ws
                SET ws.supervisor_id=subquery.SupervisorId
                FROM (select Id, SupervisorId
                FROM  users.userprofiles) AS subquery
                WHERE ws.user_id = subquery.Id;");
        }

        public override void Down()
        {
            this.Delete.Column("supervisor_id").FromTable("workspace_users");
        }
    }
}
