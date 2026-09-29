using System.ComponentModel;
using FluentMigrator;

namespace WB.Persistence.Headquarters.Migrations.Workspace
{
    [Localizable(false)]
    [Migration(202605041200)]
    public class M202605041200_Assignment_AddStatusColumn : AutoReversingMigration
    {
        public override void Up()
        {
            Create.Column("status")
                .OnTable("assignments")
                .AsInt32()
                .NotNullable()
                .WithDefaultValue(0);
        }
    }
}
