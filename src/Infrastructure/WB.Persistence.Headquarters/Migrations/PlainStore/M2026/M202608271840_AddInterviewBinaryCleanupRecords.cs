using FluentMigrator;

namespace WB.Persistence.Headquarters.Migrations.PlainStore
{
    [Migration(202608271840)]
    public class M202608271840_AddInterviewBinaryCleanupRecords : AutoReversingMigration
    {
        public override void Up()
        {
            Create.Table("interviewbinarycleanuprecords")
                .WithColumn("id").AsGuid().PrimaryKey()
                .WithColumn("interviewid").AsGuid().NotNullable()
                .WithColumn("filename").AsString(512).NotNullable()
                .WithColumn("questiontype").AsInt32().NotNullable()
                .WithColumn("requestedat").AsDateTime().NotNullable()
                .WithColumn("failedcount").AsInt32().NotNullable();

            Create.Index("ix_interviewbinarycleanuprecords_interviewid")
                .OnTable("interviewbinarycleanuprecords")
                .OnColumn("interviewid").Ascending();
        }
    }
}
