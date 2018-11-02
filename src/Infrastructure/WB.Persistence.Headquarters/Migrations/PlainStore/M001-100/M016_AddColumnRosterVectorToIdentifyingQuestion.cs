using System.ComponentModel;
using FluentMigrator;

namespace WB.Persistence.Headquarters.Migrations.PlainStore
{
    [Migration(16), Localizable(false)]
    public class M016_AddColumnRosterVectorToIdentifyingQuestion : Migration
    {
        const string assignmentsprefilledanswers = "assignmentsidentifyinganswers";
        const string rostervector = "rostervector";

        public override void Up()
        {
            this.Alter.Table(assignmentsprefilledanswers).AddColumn(rostervector).AsCustom("integer[]").Nullable();
        }

        public override void Down()
        {
            this.Delete.Column(rostervector).FromTable(assignmentsprefilledanswers);
        }
    }
}