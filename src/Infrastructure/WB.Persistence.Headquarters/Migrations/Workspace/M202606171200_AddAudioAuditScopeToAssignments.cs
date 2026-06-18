using System.ComponentModel;
using FluentMigrator;

namespace WB.Persistence.Headquarters.Migrations.Workspace
{
    [Localizable(false)]
    [Migration(202606171200)]
    public class M202606171200_AddAudioAuditScopeToAssignments : Migration
    {
        public override void Up()
        {
            Alter.Table("assignmenttoimport").AddColumn("audioauditscope").AsCustom("jsonb").Nullable();
            Alter.Table("assignments").AddColumn("audioauditscope").AsCustom("jsonb").Nullable();
        }

        public override void Down()
        {
            Delete.Column("audioauditscope").FromTable("assignmenttoimport");
            Delete.Column("audioauditscope").FromTable("assignments");
        }
    }
}
