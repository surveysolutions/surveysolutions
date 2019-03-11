using FluentMigrator;

namespace WB.UI.Designer.Migrations.PlainStore
{
    [Migration(201811071318)]
    public class M201811071318_AttachmentThumbnail : Migration
    {
        public override void Up()
        {
            this.Alter.Table(@"attachmentcontents").AddColumn("thumbnail").AsBinary().Nullable();
        }

        public override void Down()
        {
            this.Delete.Column("thumbnail").FromTable("attachmentcontents");
        }
    }
}
