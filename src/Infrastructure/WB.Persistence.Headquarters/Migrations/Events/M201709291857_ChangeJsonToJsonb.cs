using FluentMigrator;

namespace WB.Persistence.Headquarters.Migrations.Events
{
    [Migration(201709291857)]
    public class M201709291857_ChangeJsonToJsonb : Migration
    {
        public override void Up()
        {
            Execute.Sql(GetScriptToChangeJsonToJsonb($"events.events", "value"));
        }

        private static string GetScriptToChangeJsonToJsonb(string tableName, string columnName)
            => $"alter table {tableName} alter column {columnName} type jsonb using replace({columnName}::text, '\\u0000', '')::jsonb;";

        public override void Down()
        {
        }
    }
}
