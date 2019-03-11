using FluentMigrator;

namespace WB.UI.Designer.Migrations.PlainStore
{
    [Migration(17)]
    public class M017_AddQuestionnaireListViewFolderTable : Migration
    {
        public override void Up()
        {
            if (!this.Schema.Table("questionnairelistviewfolders").Exists())
            {
                this.Create.Table("questionnairelistviewfolders")
                    .WithColumn("id").AsGuid().PrimaryKey()
                    .WithColumn("title").AsString()
                    .WithColumn("parent").AsGuid().Nullable()
                    .WithColumn("createdate").AsDateTime()
                    .WithColumn("createdby").AsGuid();
            }
        }

        public override void Down()
        {
            this.Delete.Table("questionnairelistviewfolders");
        }
    }
}