using FluentMigrator;

namespace WB.UI.Designer.Migrations.PlainStore
{
    [Migration(2022_08_25_19_27)]
    public class M202208251927_AddAttachmentNameToCategories : AutoReversingMigration
    {
        public override void Up()
        {
            Create.Column("attachment_name").OnTable("categories").AsString(256).Nullable();
        }
    }
}
