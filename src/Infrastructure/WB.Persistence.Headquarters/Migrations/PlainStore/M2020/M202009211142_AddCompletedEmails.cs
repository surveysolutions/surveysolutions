using System.Data;
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
            
            if (this.Schema.Schema("readside").Exists() && this.Schema.Schema("readside").Table("interviewsummaries").Exists())
            {
                Create.ForeignKey("fk_completedemailrecords_interviewid_interviewsummaries_interviewid")
                .FromTable("completedemailrecords").InSchema("plainstore").ForeignColumn("interviewid")
                .ToTable("interviewsummaries").InSchema("readside").PrimaryColumn("interviewid")
                .OnDelete(Rule.Cascade);
            }
        }
    }
}
