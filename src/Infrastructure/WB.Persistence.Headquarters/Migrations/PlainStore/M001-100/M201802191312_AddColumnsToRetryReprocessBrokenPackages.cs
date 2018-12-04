using System.ComponentModel;
using FluentMigrator;

namespace WB.Persistence.Headquarters.Migrations.PlainStore
{
    [Localizable(false)]
    [Migration(201802191312)]
    public class M201802191312_AddColumnsToRetryReprocessBrokenPackages : Migration
    {
        public override void Up()
        {
            Alter.Table("brokeninterviewpackages")
                .AddColumn("reprocessattemptscount").AsInt32().NotNullable().SetExistingRowsTo(10);
            Alter.Table("interviewpackages")
                .AddColumn("processattemptscount").AsInt32().NotNullable().SetExistingRowsTo(0);
        }

        public override void Down()
        {
            Delete.Column("reprocessattemptscount").FromTable("brokeninterviewpackages");
            Delete.Column("processattemptscount").FromTable("interviewpackages");
        }
    }
}
