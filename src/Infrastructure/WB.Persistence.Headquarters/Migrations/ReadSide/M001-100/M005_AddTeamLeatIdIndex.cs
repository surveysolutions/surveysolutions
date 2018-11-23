using FluentMigrator;

namespace WB.Persistence.Headquarters.Migrations.ReadSide
{
    [Migration(5)]
    public class M005_AddTeamLeatIdIndex : Migration
    {
        private const string schemaName = "readside";
        private const string teamLeadIdIndexName = "interviewsummaries_teamleadid";
        private const string interviewSummariesTableName = "InterviewSummaries";
        private const string teamleadIdColumnName = "teamleadid";
        public override void Up()
        {
            if (!Schema.Table(interviewSummariesTableName).Index(teamLeadIdIndexName).Exists())
            {
                Execute.Sql($"create index {teamLeadIdIndexName} on {schemaName}.{interviewSummariesTableName} using btree({teamleadIdColumnName});");
            }

            Execute.Sql($"ALTER TABLE {schemaName}.{interviewSummariesTableName} ALTER {teamleadIdColumnName} TYPE uuid, ALTER {teamleadIdColumnName} SET NOT NULL;");
        }

        public override void Down()
        {
            Delete.Index(teamLeadIdIndexName);
        }
    }
}