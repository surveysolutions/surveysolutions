using System.ComponentModel;
using FluentMigrator;

namespace WB.Persistence.Headquarters.Migrations.PlainStore
{
    [Migration(17), Localizable(false)]
    public class M017_AlterColumnRosterVectorToIdentifyingQuestionToNullable : Migration
    {
        const string assignmentsprefilledanswers = "assignmentsidentifyinganswers";
        const string rostervector = "rostervector";

        public override void Up()
        {
            this.Update
                .Table(assignmentsprefilledanswers)
                .Set(new { rostervector = "{}" })
                .Where(new { rostervector = (int[])(null) });

            this.Alter
                .Column(rostervector).OnTable(assignmentsprefilledanswers)
                .AsCustom("integer[]").NotNullable();
        }

        public override void Down()
        {
            this.Delete.Column(rostervector).FromTable(assignmentsprefilledanswers);
        }
    }
}