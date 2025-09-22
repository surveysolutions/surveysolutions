using System.ComponentModel;
using System.Data;
using DocumentFormat.OpenXml.Drawing.Charts;
using FluentMigrator;
using FluentMigrator.Postgres;

namespace WB.Persistence.Headquarters.Migrations.Workspace
{
    [Localizable(false)]
    [Migration(202509191720)]
    public class M202509191720_AddBrokenAudioTables : Migration
    {
        public override void Up()
        {
            this.Create.Table("brokenaudiofiles")
                .WithColumn("id").AsString().PrimaryKey()
                .WithColumn("interviewid").AsGuid().NotNullable()
                .WithColumn("filename").AsString().NotNullable()
                .WithColumn("contenttype").AsString().Nullable()
                .WithColumn("data").AsBinary().NotNullable();

            this.Create.Index("brokenaudiofiles_interviewid")
                .OnTable("brokenaudiofiles")
                .OnColumn("interviewid");
        }

        public override void Down()
        {
            this.Delete.Index("brokenaudiofiles_interviewid");
            this.Delete.Table("brokenaudiofiles");
        }
    }
}
