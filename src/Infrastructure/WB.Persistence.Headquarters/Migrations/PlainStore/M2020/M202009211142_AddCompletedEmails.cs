using FluentMigrator;

namespace WB.Persistence.Headquarters.Migrations.PlainStore
{
    [Migration(202009211142)]
    public class M202009211142_AddCompletedEmails : AutoReversingMigration
    {
        public override void Up()
        {
            Create.Table("completedemailrecords")
                .WithColumn("interviewid").AsGuid().PrimaryKey()
                .WithColumn("requesttime").AsDateTime().NotNullable()
                .WithColumn("failedcount").AsInt32().NotNullable();
        }
    }
}
