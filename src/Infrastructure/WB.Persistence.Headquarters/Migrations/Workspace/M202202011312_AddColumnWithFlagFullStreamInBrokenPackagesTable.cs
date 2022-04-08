using System.ComponentModel;
using FluentMigrator;

namespace WB.Persistence.Headquarters.Migrations.Workspace
{
    [Localizable(false)]
    [Migration(202202011312)]
    public class M202202011312_AddColumnWithFlagFullStreamInBrokenPackagesTable : ForwardOnlyMigration
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
    }
}
