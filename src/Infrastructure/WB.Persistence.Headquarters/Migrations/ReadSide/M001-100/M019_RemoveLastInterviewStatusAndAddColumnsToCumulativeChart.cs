using FluentMigrator;

namespace WB.Persistence.Headquarters.Migrations.ReadSide
{
    [Migration(19)]
    public class M019_RemoveLastInterviewStatusAndAddColumnsToCumulativeChart : Migration
    {
        private static readonly string CumulativeReportStatusChangesTable = "cumulativereportstatuschanges";
        private static readonly string QuestionnaireIdentityColumn = "questionnaireidentity";
        private static readonly string InterviewIdColumn = "interviewid";
        private static readonly string EventSequenceColumn = "eventsequence";
        private static readonly string TempEntryraw = "tempentryraw";

        public override void Up()
        {
            if (Schema.Table("lastinterviewstatuses").Exists())
                Delete.Table("lastinterviewstatuses");

            Alter.Table(CumulativeReportStatusChangesTable).AddColumn(QuestionnaireIdentityColumn).AsString().Nullable();
            Alter.Table(CumulativeReportStatusChangesTable).AddColumn(InterviewIdColumn).AsGuid().Nullable();
            Alter.Table(CumulativeReportStatusChangesTable).AddColumn(EventSequenceColumn).AsInt32().Nullable();
            Alter.Table(CumulativeReportStatusChangesTable).AddColumn(TempEntryraw).AsGuid().Nullable();

            Execute.Sql($@"UPDATE readside.{CumulativeReportStatusChangesTable} 
                SET {QuestionnaireIdentityColumn}=concat(replace(questionnaireid::text, '-', ''), '$', questionnaireversion),
                    {TempEntryraw}=uuid(substr(entryid, 0, 33))");

            Execute.Sql($@"DO $$
                DECLARE doesEventsTableExists integer;
                BEGIN
                    SELECT count(tablename) FROM pg_tables WHERE schemaname = 'events' AND tablename = 'events' INTO doesEventsTableExists;
                    IF doesEventsTableExists > 0 THEN
                        UPDATE readside.{CumulativeReportStatusChangesTable}
                        SET ({InterviewIdColumn}, {EventSequenceColumn}) = (
                            SELECT eventsourceid, globalsequence
                            FROM events.events                    
                            WHERE readside.{CumulativeReportStatusChangesTable}.{TempEntryraw} = events.events.id);
                    END IF;
                END
                $$");
            
            Delete.Column(TempEntryraw).FromTable(CumulativeReportStatusChangesTable);
            Delete.Column("questionnaireid").FromTable(CumulativeReportStatusChangesTable);
            Delete.Column("questionnaireversion").FromTable(CumulativeReportStatusChangesTable);
        }

        public override void Down()
        {
            Delete.Column(QuestionnaireIdentityColumn).FromTable(CumulativeReportStatusChangesTable);
            Delete.Column(InterviewIdColumn).FromTable(CumulativeReportStatusChangesTable);
            Delete.Column(EventSequenceColumn).FromTable(CumulativeReportStatusChangesTable);

            Alter.Table(CumulativeReportStatusChangesTable).AddColumn("questionnaireid").AsGuid().Nullable();
            Alter.Table(CumulativeReportStatusChangesTable).AddColumn("questionnaireversion").AsInt64().Nullable();
            // Table is going to be created on first use
        }
    }
}