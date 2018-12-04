using System.ComponentModel;
using FluentMigrator;

namespace WB.Persistence.Headquarters.Migrations.PlainStore
{
    [Localizable(false)]
    [Migration(201710021139)]
    public class M201710021139_ChangeJsonToJsonB : Migration
    {
        public override void Up()
        {
            Execute.Sql(GetScriptToChangeJsonToJsonb("plainstore", "appsettings", "value"));
            Execute.Sql(GetScriptToChangeJsonToJsonb("plainstore", "assignments", "answers"));
            Execute.Sql(GetScriptToChangeJsonToJsonb("plainstore", "questionnairedocuments", "value"));

            Execute.Sql(GetScriptToChangeJsonToJsonb("plainstore", "webinterviewconfigs", "value"));
        }

        private static string GetScriptToChangeJsonToJsonb(string schema, string tableName, string columnName)
            => $@"DO $$
                DECLARE 
                    doesTableExists integer;
                BEGIN
                    SELECT count(tablename) FROM pg_tables WHERE schemaname = '{schema}' AND tablename = '{tableName}' INTO doesTableExists;
                    IF doesTableExists > 0 THEN
                        alter table {schema}.{tableName} alter column {columnName} type jsonb 
                            using replace({columnName}::text, '\\u0000', '')::jsonb;
                END IF;
                END
                $$"; 

        public override void Down()
        {
        }
    }
}
