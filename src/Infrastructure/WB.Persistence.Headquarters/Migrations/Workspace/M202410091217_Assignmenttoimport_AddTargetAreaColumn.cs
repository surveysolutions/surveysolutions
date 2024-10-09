using System.ComponentModel;
using FluentMigrator;
using FluentMigrator.Postgres;

namespace WB.Persistence.Headquarters.Migrations.Workspace
{
    [Localizable(false)]
    [Migration(202410091217)]
    public class M202410091217_Assignmenttoimport_AddTargetAreaColumn : AutoReversingMigration
    {
        public override void Up()
        {
            Create.Column("targetarea")
                .OnTable("assignmenttoimport")
                .AsString()
                .Nullable();
        }
    }
}
