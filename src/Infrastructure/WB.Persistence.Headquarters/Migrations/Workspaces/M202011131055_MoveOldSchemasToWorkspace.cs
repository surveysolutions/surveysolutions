using FluentMigrator;

namespace WB.Persistence.Headquarters.Migrations.Workspaces
{
    [Migration(2020_11_13_10_55)]
    public class M202011131055_MoveOldSchemasToWorkspace : Migration
    {
        internal const string primarySchemaName = "ws_primary";

        public override void Up()
        {
            Alter.Table("events").InSchema("events").ToSchema(primarySchemaName);

            string[] plainStoreTables =
            {
                "appsettings", "assemblyinfos", "assignmentsimportprocess",
                "assignmenttoimport",
                "attachmentcontents", "audioauditfiles", "audiofiles", "auditlogrecords",
                "brokeninterviewpackages", "completedemailrecords", "deviceinfos",
                "devicesyncinfo", "devicesyncstatistics", "featuredquestions",
                "hibernate_unique_key", "interviewpackages", "invitations",
                "mapbrowseitems", "productversionhistory", "profilesettings",
                "questionnairebackups", "questionnairebrowseitems", "questionnairedocuments",
                "questionnairelookuptables", "questionnairepdfs", "receivedpackagelogentries",
                "reusablecategoricaloptions", "synchronizationlog", "systemlog", "tablet_logs",
                "translationinstances", "usermaps", "usersimportprocess",
                "usertoimport", "webinterviewconfigs"
            };

            MoveToPrimarySchema(plainStoreTables, "plainstore");

            string[] readSideTables =
            {
                "assignments", "assignmentsidentifyinganswers", "commentaries", "cumulativereportstatuschanges",
                "identifyingentityvalue", "interview_geo_answers", "interviewcommentedstatuses", "interviewflags",
                "interviewsummaries", "questionnaire_entities", "report_statistics", "timespanbetweenstatuses"
            };

            MoveToPrimarySchema(readSideTables, "readside");
        }

        private void MoveToPrimarySchema(string[] plainStoreTables, string schemaName)
        {
            foreach (var table in plainStoreTables)
            {
                Alter.Table(table).InSchema(schemaName).ToSchema(primarySchemaName);
            }
        }

        public override void Down()
        {
        }
    }
}
