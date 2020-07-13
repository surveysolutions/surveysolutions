using FluentMigrator;

namespace WB.Persistence.Headquarters.Migrations.PlainStore
{
    [Migration(202007121245)]
    public class M202007121245_FixInvitationsWithoutInterview : Migration
    {
        public override void Up()
        {
            if (this.Schema.Schema("readside").Exists())
            {
                if (this.Schema.Schema("readside").Table("interviewsummaries").Exists())
                {
                    Execute.Sql(@"update plainstore.invitations
                set interviewid = null
                where interviewid in (
	                select i.interviewid 
	                from plainstore.invitations i
	                left join readside.interviewsummaries s
		                on s.summaryid = i.interviewid 
	                where s.summaryid is null and i.interviewid is not null)");
                }
            }
        }

        public override void Down()
        {
            
        }
    }
}
