using FluentMigrator;

namespace WB.Persistence.Headquarters.Migrations.MigrateToPrimaryWorkspace
{
    [Migration(2020_11_13_13_16)]
    public class M202011131316_MigrateLeftovers : ForwardOnlyMigration
    {
        public override void Up()
        {
            Execute.Sql($"ALTER SEQUENCE events.globalsequence SET SCHEMA {M202011131055_MoveOldSchemasToWorkspace.primarySchemaName}");
            Execute.Sql($"ALTER SEQUENCE plainstore.assignment_id_sequence SET SCHEMA {M202011131055_MoveOldSchemasToWorkspace.primarySchemaName}");
            
            Execute.Sql(string.Format(@"CREATE OR REPLACE VIEW {0}.report_statistics_categorical
AS SELECT a.interview_id AS interview_id,
     a.entity_id,
     a.rostervector,
     ans.ans AS answer
   FROM {0}.report_statistics a,
   LATERAL unnest(a.answer) ans(ans)
   where a.""type"" = 0 and is_enabled = true;", M202011131055_MoveOldSchemasToWorkspace.primarySchemaName));

            Execute.Sql(string.Format(@"CREATE OR REPLACE VIEW {0}.report_statistics_numeric
AS SELECT a.interview_id AS interview_id,
     a.entity_id,
     a.rostervector,
     ans.ans AS answer
   FROM {0}.report_statistics a,
   LATERAL unnest(a.answer) ans(ans)
   where a.""type"" = 1 and is_enabled = true;", M202011131055_MoveOldSchemasToWorkspace.primarySchemaName));
            
            Execute.Sql($@" -- https://wiki.postgresql.org/wiki/Aggregate_Median
                CREATE OR REPLACE FUNCTION {M202011131055_MoveOldSchemasToWorkspace.primarySchemaName}._final_median(NUMERIC[])
                   RETURNS NUMERIC AS
                $$
                   SELECT AVG(val)
                   FROM (
                     SELECT val
                     FROM unnest($1) val
                     ORDER BY 1
                     LIMIT  2 - MOD(array_upper($1, 1), 2)
                     OFFSET CEIL(array_upper($1, 1) / 2.0) - 1
                   ) sub;
                $$
                LANGUAGE 'sql' IMMUTABLE;
                 
                CREATE AGGREGATE {M202011131055_MoveOldSchemasToWorkspace.primarySchemaName}.median(NUMERIC) (
                  SFUNC=array_append,
                  STYPE=NUMERIC[],
                  FINALFUNC={M202011131055_MoveOldSchemasToWorkspace.primarySchemaName}._final_median,
                  INITCOND='{{}}'
                );");
        }
    }
}
