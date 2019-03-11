using FluentMigrator;

namespace WB.UI.Designer.Migrations.PlainStore
{
    [Migration(18)]
    public class M18_AddFolderIdToQuestionnaireListViewItemTable : Migration
    {
        public override void Up()
        {
            var isExists = this.Schema.Table("questionnairelistviewitems").Column("folderid").Exists();

            if (!isExists)
            {
                this.Create.Column("folderid").OnTable("questionnairelistviewitems").AsGuid().Nullable();
            }
        }

        public override void Down()
        {
            this.Delete.Column("folderid").FromTable("questionnairelistviewitems");
        }
    }
}