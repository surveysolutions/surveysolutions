using System.ComponentModel;
using FluentMigrator;

namespace WB.Persistence.Headquarters.Migrations.Workspace
{
    [Localizable(false)]
    [Migration(202605061430, TransactionBehavior.None)]
    public class M202605061430_InterviewSearch_AddTrigramIndexes : ForwardOnlyMigration
    {
        public override void Up()
        {
            // Ensure pg_trgm extension operators are available from all workspace schemas.
            Execute.Sql(@"
                DO $$
                BEGIN
                    CREATE EXTENSION IF NOT EXISTS pg_trgm WITH SCHEMA public;
                    IF EXISTS (
                        SELECT 1
                        FROM pg_extension e
                        JOIN pg_namespace n ON n.oid = e.extnamespace
                        WHERE e.extname = 'pg_trgm' AND n.nspname <> 'public'
                    ) THEN
                        ALTER EXTENSION pg_trgm SET SCHEMA public;
                    END IF;
                END
                $$;");

            // GIN trigram indexes on interviewsummaries fields used in substring search
            Execute.Sql(@"CREATE INDEX CONCURRENTLY IF NOT EXISTS interviewsummaries_key_trgm_idx
                ON interviewsummaries USING GIN (key public.gin_trgm_ops);");

            Execute.Sql(@"CREATE INDEX CONCURRENTLY IF NOT EXISTS interviewsummaries_clientkey_trgm_idx
                ON interviewsummaries USING GIN (clientkey public.gin_trgm_ops);");

            Execute.Sql(@"CREATE INDEX CONCURRENTLY IF NOT EXISTS interviewsummaries_responsible_name_lower_case_trgm_idx
                ON interviewsummaries USING GIN (responsible_name_lower_case public.gin_trgm_ops);");

            Execute.Sql(@"CREATE INDEX CONCURRENTLY IF NOT EXISTS interviewsummaries_teamlead_name_lower_case_trgm_idx
                ON interviewsummaries USING GIN (teamlead_name_lower_case public.gin_trgm_ops);");

            // GIN trigram index on identifyingentityvalue used in substring search
            Execute.Sql(@"CREATE INDEX CONCURRENTLY IF NOT EXISTS identifyingentityvalue_value_lower_case_trgm_idx
                ON identifyingentityvalue USING GIN (value_lower_case public.gin_trgm_ops);");
        }
    }
}
