using System.ComponentModel;
using FluentMigrator;
using FluentMigrator.Postgres;

namespace WB.Persistence.Headquarters.Migrations.Workspace
{
    [Localizable(false)]
    [Migration(202206082042)]
    public class M202206082042_Invitations_AddIndices : AutoReversingMigration
    {
        public override void Up()
        {
            Create.Index("invitations_assignmentid_indx")
                .OnTable("invitations")
                .OnColumn("assignmentid")
                .Ascending()
                .NullsLast();
            
            Create.Index("invitations_token_indx")
                .OnTable("invitations")
                .OnColumn("token")
                .Ascending()
                .NullsLast();
        }
    }
}
