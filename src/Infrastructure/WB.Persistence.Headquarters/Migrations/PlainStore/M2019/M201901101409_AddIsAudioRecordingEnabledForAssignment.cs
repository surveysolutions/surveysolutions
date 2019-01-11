using System.ComponentModel;
using FluentMigrator;

namespace WB.Persistence.Headquarters.Migrations.PlainStore
{
    [Migration(201901101409)]
    [Localizable(false)]
    public class M201901101409_AddIsAudioRecordingEnabledForAssignment : Migration
    {
        public override void Up()
        {
            Create.Column("isaudiorecordingenabled").OnTable("assignments").AsBoolean().NotNullable().WithDefaultValue(false);
        }

        public override void Down()
        {
            Delete.Column("isaudiorecordingenabled").FromTable("assignments");
        }
    }
}
