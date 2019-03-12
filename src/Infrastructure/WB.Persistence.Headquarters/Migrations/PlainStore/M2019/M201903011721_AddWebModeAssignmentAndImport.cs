using System.ComponentModel;
using FluentMigrator;

namespace WB.Persistence.Headquarters.Migrations.PlainStore
{
    [Migration(201903011721)]
    [Localizable(false)]
    public class M201903011721_AddWebModeAssignmentAndImport : Migration
    {
        public override void Up()
        {
            if (!Schema.Table("assignments").Column("webmode").Exists())
            {
                Create.Column("webmode").OnTable("assignments").AsBoolean().Nullable();
            }

            if (!Schema.Table("assignmenttoimport").Column("webmode").Exists())
            {
                Create.Column("webmode").OnTable("assignmenttoimport").AsBoolean().Nullable();
            }
        }

        public override void Down()
        {
            Delete.Column("webmode").FromTable("assignments");
            Delete.Column("webmode").FromTable("assignmenttoimport");
        }
    }
}
