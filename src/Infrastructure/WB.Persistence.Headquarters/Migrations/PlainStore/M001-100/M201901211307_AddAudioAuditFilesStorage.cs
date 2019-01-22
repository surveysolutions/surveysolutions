using FluentMigrator;

namespace WB.Persistence.Headquarters.Migrations.PlainStore
{
    [Migration(201901211307)]
    public class M201901211307_AddAudioAuditFilesStorage : Migration
    {
        public override void Up()
        {
            this.Create.Table("audioauditfiles")
                .WithColumn("id").AsString().PrimaryKey()
                .WithColumn("interviewid").AsGuid().NotNullable()
                .WithColumn("filename").AsString().NotNullable()
                .WithColumn("contenttype").AsString().Nullable()
                .WithColumn("data").AsBinary().NotNullable();

            this.Create.Index("audioauditfiles_interviewid")
                .OnTable("audioauditfiles")
                .OnColumn("interviewid");
        }

        public override void Down()
        {
            this.Delete.Index("audioauditfiles_interviewid");
            this.Delete.Table("audioauditfiles");
        }
    }
}
