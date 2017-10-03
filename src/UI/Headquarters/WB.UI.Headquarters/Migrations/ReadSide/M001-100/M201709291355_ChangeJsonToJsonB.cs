using System.ComponentModel;
using FluentMigrator;

namespace WB.UI.Headquarters.Migrations.ReadSide
{
    [Localizable(false)]
    [Migration(201709291355)]
    public class M201709291355_ChangeJsonToJsonB : Migration
    {
        public override void Up()
        {
            Execute.Sql(GetScriptToChangeJsonToJsonb("readside", "interviews", "asintmatrix"));
            Execute.Sql(GetScriptToChangeJsonToJsonb("readside", "interviews", "asgps"));
            Execute.Sql(GetScriptToChangeJsonToJsonb("readside", "interviews", "asyesno"));
            Execute.Sql(GetScriptToChangeJsonToJsonb("readside", "interviews", "asaudio"));
            Execute.Sql(GetScriptToChangeJsonToJsonb("readside", "interviews", "asarea"));
            Execute.Sql(GetScriptToChangeJsonToJsonb("readside", "interviews", "aslist"));
        }

        private static string GetScriptToChangeJsonToJsonb(string schema, string tableName, string columnName)
            => $@"DO $$
                DECLARE 
                    doesTableExists integer;
                BEGIN
                    SELECT count(tablename) FROM pg_tables WHERE schemaname = '{schema}' AND tablename = '{tableName}' INTO doesTableExists;
                    IF doesTableExists > 0 THEN
                        alter table {schema}.{tableName} alter column {columnName} type jsonb using {columnName}::text::jsonb;
                END IF;
                END
                $$";

        public override void Down()
        {
        }
    }
}