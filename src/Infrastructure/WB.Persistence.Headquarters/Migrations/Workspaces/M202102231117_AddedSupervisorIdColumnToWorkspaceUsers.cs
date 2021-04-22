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
                UPDATE workspaces.workspace_users
                SET supervisor_id=subquery.""SupervisorId""
                FROM (select uu.""Id"", up.""SupervisorId""
                    FROM  users.userprofiles up INNER JOIN users.users uu ON uu.""UserProfileId"" = up.""Id"") AS subquery
                WHERE user_id = subquery.""Id"";");
        }

        public override void Down()
        {
            this.Delete.Column("supervisor_id").FromTable("workspace_users");
        }
    }
}
