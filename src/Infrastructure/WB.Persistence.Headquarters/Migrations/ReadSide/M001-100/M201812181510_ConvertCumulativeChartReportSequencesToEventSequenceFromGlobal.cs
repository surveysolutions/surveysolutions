using System.Diagnostics.CodeAnalysis;
using FluentMigrator;

namespace WB.Persistence.Headquarters.Migrations.ReadSide
{
    [Migration(201812181510)]
    [SuppressMessage("ReSharper", "StringLiteralTypo")]
    public class M201812181510_ConvertCumulativeChartReportSequencesFromGlobalEventSequenceToLocal : Migration
    {
        public override void Up()
        {
            if (Schema.Schema("events").Exists())
            {
                Execute.Sql(@"update
	                readside.cumulativereportstatuschanges cum
                set
	                eventsequence = ev.eventsequence
                from
	                events.events ev
                where
	                ev.globalsequence = cum.eventsequence");
            }
        }

        public override void Down()
        {
            Execute.Sql(@"update
	                readside.cumulativereportstatuschanges cum
                set
	                eventsequence = ev.globalsequence
                from
	                events.events ev
                where
	                ev.eventsequence = cum.eventsequence and ev.eventsourceid = cum.interviewid");
        }
    }
}
