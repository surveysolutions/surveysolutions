using System.Linq;
using Dapper;
using FluentMigrator;

namespace WB.Persistence.Headquarters.Migrations.MigrateToPrimaryWorkspace
{
    [Migration(2020_11_13_10_55)]
    public class M202011131055_MoveOldSchemasToWorkspace : Migration
    {
        internal const string primarySchemaName = "ws_primary";

        public override void Up()
        {
            Alter.Table("events").InSchema("events").ToSchema(primarySchemaName);

            string sqlFormat =
                @"DO
                $$DECLARE
            p_table regclass;
            BEGIN
                SET LOCAL search_path='{0}';
            FOR p_table IN
                SELECT oid FROM pg_class
            WHERE relnamespace = '{0}'::regnamespace
            AND relkind = 'r' AND relname NOT IN('VersionInfo', 'hibernate_unique_key', 'productversionhistory') 
            LOOP
                EXECUTE format('ALTER TABLE %s SET SCHEMA ws_primary', p_table);
            END LOOP;
            END;$$;";

            Execute.Sql(string.Format(sqlFormat, "plainstore"));
            Execute.Sql(string.Format(sqlFormat, "readside"));
            Execute.Sql("CREATE SCHEMA IF NOT EXISTS workspaces");
            Execute.Sql("ALTER TABLE plainstore.productversionhistory SET SCHEMA workspaces");
            Create.Table("hibernate_unique_key")
                .WithColumn("next_hi").AsInt32().NotNullable();
            
            
            
            
            Execute.WithConnection((c, t) =>
            {
                var maxValue =
                    c.Query<int>(
                        "SELECT MAX(next_hi) FROM readside.hibernate_unique_key UNION SELECT MAX(next_hi) FROM plainstore.hibernate_unique_key;")
                    .Max();

                c.ExecuteScalar("INSERT INTO ws_primary.hibernate_unique_key VALUES (:val)", new {val = maxValue});
            });
            
        }

        public override void Down()
        {
        }
    }
}
