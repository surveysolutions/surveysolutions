using System.ComponentModel;
using FluentMigrator;

namespace WB.Persistence.Headquarters.Migrations.PlainStore
{
    [Migration(201811161531)]
    [Localizable(false)]
    public class M201811161531_AddFileNameColumnToAttachments : Migration
    {
        public override void Up()
        {
            Create.Column("filename").OnTable("attachmentcontents").AsString().Nullable();
        }

        public override void Down()
        {
            Delete.Column("filename").FromTable("attachmentcontents");
        }
    }
}
