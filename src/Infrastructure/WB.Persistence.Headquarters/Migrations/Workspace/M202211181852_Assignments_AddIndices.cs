using System.ComponentModel;
using FluentMigrator;
using FluentMigrator.Postgres;

namespace WB.Persistence.Headquarters.Migrations.Workspace
{
    [Localizable(false)]
    [Migration(202211181852)]
    public class M202211181852_Assignments_AddIndices : AutoReversingMigration
    {
        public override void Up()
        {
            var assignments = "assignments";
            var assignments_updatedatutc_indx = "assignments_updatedatutc_indx";

            if (!Schema.Table(assignments).Index(assignments_updatedatutc_indx).Exists())
            {
                Create.Index(assignments_updatedatutc_indx)
                    .OnTable(assignments)
                    .OnColumn("updatedatutc")
                    .Ascending()
                    .NullsLast();
            }
        }
    }
}
