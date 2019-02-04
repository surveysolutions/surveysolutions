using FluentMigrator;

namespace WB.UI.Designer.Migrations.PlainStore
{
    [Migration(201901111638)]
    public class M201901111638_AddIndexOnTypeColumnInClassificationsTable : Migration
    {
        private const string tableName = "classificationentities";
        private const string columnName = "type";
        private readonly string indexName = $"{columnName}_{tableName}_idx";

        public override void Up()
        {
            this.Create.Index(indexName)
                .OnTable(tableName)
                .OnColumn(columnName);
        }

        public override void Down()
        {
            this.Delete.Index(indexName).OnTable(tableName);
        }
    }
}