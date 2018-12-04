using System.ComponentModel;
using FluentMigrator;

namespace WB.Persistence.Headquarters.Migrations.PlainStore
{
    [Migration(18), Localizable(false)]
    public class M018_AddColumnAnswersToAssignments : Migration
    {
        const string assignments = "assignments";
        const string answers = "answers";

        public override void Up()
        {
            this.Create.Column(answers)
                .OnTable(assignments)
                .AsCustom("json")
                .Nullable();
        }

        public override void Down()
        {
            this.Delete.Column(answers)
                .FromTable(assignments);
        }
    }
}