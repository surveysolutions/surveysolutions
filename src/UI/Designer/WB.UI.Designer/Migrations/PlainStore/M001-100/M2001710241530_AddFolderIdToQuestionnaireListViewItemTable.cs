using FluentMigrator;

namespace WB.UI.Designer.Migrations.PlainStore
{
    [Migration(2001710241530)]
    public class M2001710241530_AddFolderIdToQuestionnaireListViewItemTable : Migration
    {
        public override void Up()
        {
            this.Create.Column("folderid").OnTable("questionnairelistviewitems").AsGuid().Nullable();
        }

        public override void Down()
        {
            this.Delete.Column("folderid").FromTable("questionnairelistviewitems");
        }
    }
}