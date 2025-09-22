using System.ComponentModel;
using System.Data;
using DocumentFormat.OpenXml.Drawing.Charts;
using FluentMigrator;
using FluentMigrator.Postgres;

namespace WB.Persistence.Headquarters.Migrations.Workspace
{
    [Localizable(false)]
    [Migration(202509191721)]
    public class M202509191721_AddBrokenAudioAuditTables : Migration
    {
        public override void Up()
        {
            this.Create.Table("brokenaudioauditfiles")
                .WithColumn("id").AsString().PrimaryKey()
                .WithColumn("interviewid").AsGuid().NotNullable()
                .WithColumn("filename").AsString().NotNullable()
                .WithColumn("contenttype").AsString().Nullable()
                .WithColumn("data").AsBinary().NotNullable();

            this.Create.Index("brokenaudioauditfiles_interviewid")
                .OnTable("brokenaudioauditfiles")
                .OnColumn("interviewid");
        }

        public override void Down()
        {
            this.Delete.Index("brokenaudioauditfiles_interviewid");
            this.Delete.Table("brokenaudioauditfiles");
        }
    }
}
