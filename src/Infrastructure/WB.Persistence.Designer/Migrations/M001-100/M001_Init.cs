using FluentMigrator;

namespace WB.UI.Designer.Migrations.PlainStore
{
    [Migration(1)]
    public class M001_Init : Migration
    {
        public override void Up()
        {
            Create.Table("attachmentcontents")
                .WithColumn("id").AsString(255).PrimaryKey()
                .WithColumn("content").AsBinary().Nullable()
                .WithColumn("size").AsInt64().Nullable()
                .WithColumn("attachmentheight").AsInt32().Nullable()
                .WithColumn("attachmentwidth").AsInt32().Nullable()
                .WithColumn("contenttype").AsString().Nullable();

            Create.Table("attachmentmetas")
                .WithColumn("id").AsGuid().PrimaryKey()
                .WithColumn("questionnaireid").AsGuid().Nullable()
                .WithColumn("contentid").AsString().Nullable()
                .WithColumn("filename").AsString().Nullable()
                .WithColumn("lastupdatedate").AsDateTime().Nullable();

            Create.Table("productversionhistory")
                .WithColumn("updatetimeutc").AsDateTime().PrimaryKey()
                .WithColumn("productversion").AsString().Nullable();

            Create.Index("attachmentmeta_questionnaireid").OnTable("attachmentmetas").OnColumn("questionnaireid");
            Create.Index("attachmentmeta_contentid").OnTable("attachmentmetas").OnColumn("contentid");

            Create.Table("hibernate_unique_key")
              .WithColumn("next_hi").AsInt32();

            Insert.IntoTable("hibernate_unique_key").Row(new { next_hi = 1 });
        }

        public override void Down()
        {
            Delete.Table("AttachmentContents");
            Delete.Table("AttachmentMetas");
            Delete.Table("ProductVersionHistory");
        }
    }
}