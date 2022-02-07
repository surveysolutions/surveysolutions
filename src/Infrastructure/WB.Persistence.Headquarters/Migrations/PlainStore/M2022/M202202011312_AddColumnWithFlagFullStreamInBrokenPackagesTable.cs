using System.ComponentModel;
using FluentMigrator;

namespace WB.Persistence.Headquarters.Migrations.PlainStore
{
    [Localizable(false)]
    [Migration(202202011312)]
    public class M202202011312_AddColumnWithFlagFullStreamInBrokenPackagesTable : Migration
    {
        public override void Up()
        {
            Alter.Table("brokeninterviewpackages")
                .AddColumn("isfulleventstream")
                .AsBoolean()
                .NotNullable()
                .WithDefaultValue(false)
                .SetExistingRowsTo(false);
            Alter.Table("interviewpackages")
                .AddColumn("isfulleventstream")
                .AsBoolean()
                .NotNullable()
                .WithDefaultValue(false)
                .SetExistingRowsTo(false);
        }

        public override void Down()
        {
            Delete.Column("isfulleventstream").FromTable("brokeninterviewpackages");
            Delete.Column("isfulleventstream").FromTable("interviewpackages");
        }
    }
}
