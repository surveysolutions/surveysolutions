using FluentMigrator;

namespace WB.Persistence.Headquarters.Migrations.ReadSide
{
    [Migration(201709071637)]
    public class M201709071637_AddIndexToCumulativeReportStatusChangesTable : Migration
    {
        private static readonly string CumulativeReportStatusChangesTable = "cumulativereportstatuschanges";
        private static readonly string EventSequenceColumn = "eventsequence";

        public override void Up()
        {
            Create.Index($"{CumulativeReportStatusChangesTable}_{EventSequenceColumn}_indx").OnTable(CumulativeReportStatusChangesTable).OnColumn(EventSequenceColumn).Descending();
        }

        public override void Down()
        {
            this.Delete.Index($"{CumulativeReportStatusChangesTable}_{EventSequenceColumn}_indx");
        }
    }
}