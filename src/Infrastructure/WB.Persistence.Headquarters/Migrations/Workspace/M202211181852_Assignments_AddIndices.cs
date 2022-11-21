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
    
    [Localizable(false)]
    [Migration(202211181854)]
    public class M202211181854_AssignmentsIdentifyingAnswers_AddIndices : Migration
    {
        public override void Up()
        {
            var assignmentsidentifyinganswers = "assignmentsidentifyinganswers";
            var assignmentsidentifyinganswers_answerasstring_lower_indx = "assignmentsidentifyinganswers_answerasstring_lower_indx";

            Execute.Sql($"create index if not exists {assignmentsidentifyinganswers_answerasstring_lower_indx} ON {assignmentsidentifyinganswers} (lower(answerasstring))");
        }

        public override void Down()
        {
        }
    }
}
