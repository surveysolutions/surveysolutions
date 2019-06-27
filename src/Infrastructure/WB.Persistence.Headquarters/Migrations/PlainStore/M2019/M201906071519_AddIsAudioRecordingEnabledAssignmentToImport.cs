using System.ComponentModel;
using FluentMigrator;

namespace WB.Persistence.Headquarters.Migrations.PlainStore
{
    [Migration(201906071519)]
    [Localizable(false)]
    public class M201906071519_AddIsAudioRecordingEnabledAssignmentToImport : Migration
    {
        public override void Up()
        {
            if (!Schema.Table("assignmenttoimport").Column("isaudiorecordingenabled").Exists())
            {
                Create.Column("isaudiorecordingenabled").OnTable("assignmenttoimport").AsBoolean().Nullable();
            }
        }

        public override void Down()
        {
            Delete.Column("isaudiorecordingenabled").FromTable("assignmenttoimport");
        }
    }
}
