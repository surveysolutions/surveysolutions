using System.ComponentModel;
using FluentMigrator;
using FluentMigrator.Postgres;

namespace WB.Persistence.Headquarters.Migrations.Workspace
{
    [Localizable(false)]
    [Migration(202410101115)]
    public class M202410101115_Assignment_AddTargetAreaColumn : AutoReversingMigration
    {
        public override void Up()
        {
            Create.Column("targetarea")
                .OnTable("assignments")
                .AsString()
                .Nullable();
        }
    }
}
