using System;
using System.ComponentModel;
using System.Data;
using Dapper;
using FluentMigrator;

namespace WB.Persistence.Headquarters.Migrations.Workspace
{
    [Localizable(false)]
    [Migration(202605061430, TransactionBehavior.None)]
    public class M202605061430_InterviewSearch_AddTrigramIndexes : ForwardOnlyMigration
    {
        public override void Up()
        {
            Execute.WithConnection((connection, _) =>
            {
                var ginTrgmOperatorClass = connection.QuerySingleOrDefault<string>(@"
                    SELECT format('%I.gin_trgm_ops', n.nspname)
                    FROM pg_extension e
                    JOIN pg_namespace n ON n.oid = e.extnamespace
                    WHERE e.extname = 'pg_trgm'");
                if (ginTrgmOperatorClass == null)
                    throw new InvalidOperationException(
                        "The pg_trgm extension must be installed by a database administrator before interview search indexes can be created.");

                DropInvalidIndex(connection, "interviewsummaries_key_trgm_idx");
                DropInvalidIndex(connection, "interviewsummaries_clientkey_trgm_idx");
                DropInvalidIndex(connection, "interviewsummaries_responsible_name_lower_case_trgm_idx");
                DropInvalidIndex(connection, "interviewsummaries_teamlead_name_lower_case_trgm_idx");
                DropInvalidIndex(connection, "identifyingentityvalue_value_lower_case_trgm_idx");

                // GIN trigram indexes on interviewsummaries fields used in substring search
                connection.Execute($@"CREATE INDEX CONCURRENTLY IF NOT EXISTS interviewsummaries_key_trgm_idx
                    ON interviewsummaries USING GIN (key {ginTrgmOperatorClass});");

                connection.Execute($@"CREATE INDEX CONCURRENTLY IF NOT EXISTS interviewsummaries_clientkey_trgm_idx
                    ON interviewsummaries USING GIN (clientkey {ginTrgmOperatorClass});");

                connection.Execute($@"CREATE INDEX CONCURRENTLY IF NOT EXISTS interviewsummaries_responsible_name_lower_case_trgm_idx
                    ON interviewsummaries USING GIN (responsible_name_lower_case {ginTrgmOperatorClass});");

                connection.Execute($@"CREATE INDEX CONCURRENTLY IF NOT EXISTS interviewsummaries_teamlead_name_lower_case_trgm_idx
                    ON interviewsummaries USING GIN (teamlead_name_lower_case {ginTrgmOperatorClass});");

                // GIN trigram index on identifyingentityvalue used in substring search
                connection.Execute($@"CREATE INDEX CONCURRENTLY IF NOT EXISTS identifyingentityvalue_value_lower_case_trgm_idx
                    ON identifyingentityvalue USING GIN (value_lower_case {ginTrgmOperatorClass});");
            });
        }

        private static void DropInvalidIndex(IDbConnection connection, string indexName)
        {
            var invalidIndex = connection.QuerySingleOrDefault<string>(@"
                SELECT format('%I.%I', index_schema.nspname, index_class.relname)
                FROM pg_index
                JOIN pg_class index_class ON index_class.oid = pg_index.indexrelid
                JOIN pg_namespace index_schema ON index_schema.oid = index_class.relnamespace
                WHERE index_class.relname = @indexName
                    AND index_schema.nspname = current_schema()
                    AND NOT pg_index.indisvalid",
                new { indexName });

            if (invalidIndex != null)
                connection.Execute($"DROP INDEX CONCURRENTLY IF EXISTS {invalidIndex}");
        }
    }
}
