using System.ComponentModel;
using FluentMigrator;

namespace WB.Persistence.Headquarters.Migrations.PlainStore
{
    [Migration(19), Localizable(false)]
    public class M019_AddAudioFileStorage : Migration
    {
        public override void Up()
        {
            this.Create.Table("audiofiles")
                .WithColumn("id").AsString().PrimaryKey()
                .WithColumn("interviewid").AsGuid().NotNullable()
                .WithColumn("filename").AsString().NotNullable()
                .WithColumn("data").AsBinary().NotNullable();

            this.Create.Index("audiofiles_interviewid")
                .OnTable("audiofiles")
                .OnColumn("interviewid");
        }

        public override void Down()
        {
            this.Delete.Index("audiofiles_interviewid");
            this.Delete.Table("audiofiles");
        }
    }
}