using System.ComponentModel;
using FluentMigrator;

namespace WB.Persistence.Headquarters.Migrations.ReadSide
{
    [Migration(201802011630, TransactionBehavior.None)]
    [Localizable(false)]
    public class M201802011630_SpeedupMapReport : Migration
    {
        public override void Up()
        {
           // Execute.Sql("CREATE INDEX CONCURRENTLY IF NOT Exists interviews_asgps_not_null_indx ON readside.interviews(asgps) WHERE asgps IS NOT NULL;");
        }

        public override void Down()
        {
        }
    }
}