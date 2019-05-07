using FluentMigrator;

namespace WB.UI.Designer.Migrations.PlainStore
{
    [Migration(201904091312)]
    public class M201904091312_AddMissingKeyValueTables : AutoReversingMigration
    {
        public override void Up()
        {
            CreateKeyValueTable("questionnairedocuments");
            CreateKeyValueTable("lookuptablecontents");
            CreateKeyValueTable("questionnairestatetrackers");
        }

        private void CreateKeyValueTable(string tableName)
        {
            if (!Schema.Table(tableName).Exists())
            {
                Create.Table(tableName).WithColumn("id").AsString().PrimaryKey().WithColumn("value")
                    .AsCustom("json");
            }
        }
    }
}
