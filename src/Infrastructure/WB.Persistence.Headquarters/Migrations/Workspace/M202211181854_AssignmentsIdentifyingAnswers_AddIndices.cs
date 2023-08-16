using System.ComponentModel;
using FluentMigrator;

namespace WB.Persistence.Headquarters.Migrations.Workspace;

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
