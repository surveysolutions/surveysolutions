using FluentMigrator;

namespace WB.UI.Designer.Migrations.PlainStore
{
    [Migration(201812181637)]
    public class M201812181637_AddQuestionnaireSearchEntities : Migration
    {
        private string tableName = "questionnairesearchentities";
        private string tableNameWithShema = "plainstore.questionnairesearchentities";

        public override void Up()
        {
            this.Create.Table(tableName)
                .WithColumn("title").AsString()
                .WithColumn("questionnaireid").AsGuid()
                .WithColumn("entityid").AsGuid()
                .WithColumn("entitytype").AsString()
                .WithColumn("searchtext").AsCustom("tsvector");


            this.Create.PrimaryKey($"primarykey_{tableName}")
                .OnTable(tableName)
                .Columns("questionnaireid", "entityid");


            this.Execute.Sql($"CREATE INDEX searchtext_{tableName}_idx ON {tableNameWithShema} USING GIN (searchtext);");
        }

        public override void Down()
        {
            this.Delete.Table(tableName);
        }
    }
}
