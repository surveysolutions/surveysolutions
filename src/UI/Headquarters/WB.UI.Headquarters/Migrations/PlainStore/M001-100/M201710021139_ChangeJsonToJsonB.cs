using System.ComponentModel;
using FluentMigrator;

namespace WB.UI.Headquarters.Migrations.PlainStore
{
    [Localizable(false)]
    [Migration(201710021139)]
    public class M201710021139_ChangeJsonToJsonB : Migration
    {
        public override void Up()
        {
            Execute.Sql(GetScriptToChangeJsonToJsonb("plainstore.appsettings", "value"));
            Execute.Sql(GetScriptToChangeJsonToJsonb("plainstore.assignments", "answers"));
            Execute.Sql(GetScriptToChangeJsonToJsonb("plainstore.questionnairedocuments", "value"));

            Execute.Sql(GetScriptToChangeJsonToJsonb("plainstore.webinterviewconfigs", "value"));
        }

        private static string GetScriptToChangeJsonToJsonb(string tableName, string columnName)
            => $"alter table {tableName} alter column {columnName} type jsonb using {columnName}::text::jsonb;";

        public override void Down()
        {
        }
    }
}