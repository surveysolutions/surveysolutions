using System.ComponentModel;
using FluentMigrator;

namespace WB.Persistence.Headquarters.Migrations.Workspace
{
    [Localizable(false)]
    [Migration(202605061430)]
    public class M202605061430_InterviewSearch_AddTrigramIndexes : ForwardOnlyMigration
    {
        public override void Up()
        {
            // Enable pg_trgm extension (database-level, idempotent)
            Execute.Sql("CREATE EXTENSION IF NOT EXISTS pg_trgm;");

            // GIN trigram indexes on interviewsummaries fields used in substring search
            Execute.Sql(@"CREATE INDEX IF NOT EXISTS interviewsummaries_key_trgm_idx
                ON interviewsummaries USING GIN (key gin_trgm_ops);");

            Execute.Sql(@"CREATE INDEX IF NOT EXISTS interviewsummaries_clientkey_trgm_idx
                ON interviewsummaries USING GIN (clientkey gin_trgm_ops);");

            Execute.Sql(@"CREATE INDEX IF NOT EXISTS interviewsummaries_responsible_name_lower_case_trgm_idx
                ON interviewsummaries USING GIN (responsible_name_lower_case gin_trgm_ops);");

            // GIN trigram index on identifyingentityvalue used in substring search
            Execute.Sql(@"CREATE INDEX IF NOT EXISTS identifyingentityvalue_value_lower_case_trgm_idx
                ON identifyingentityvalue USING GIN (value_lower_case gin_trgm_ops);");
        }
    }
}
