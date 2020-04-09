using FluentMigrator;

namespace WB.Persistence.Headquarters.Migrations.PlainStore
{
    [Migration(2020_04_07_13_34)]
    public class M202004071334_ExtendInvitations : Migration
    {
        public override void Up()
        {
            Create.Column("last_rejected_interview_email_id").OnTable("invitations")
                .AsString().Nullable();
            Create.Column("last_rejected_status_position").OnTable("invitations")
                .AsInt32().Nullable();

            if (Schema.Schema("readside").Table("interviewcommentedstatuses").Exists())
            {
                Execute.Sql(@"update plainstore.invitations 
            set last_rejected_status_position = ss.""position""
            from (select max(ics.""position"") as ""position"", s.summaryid as interview_id 
            from readside.interviewcommentedstatuses ics
            inner join readside.interviewsummaries s on ics.interview_id = s.id 
            where ics.status = 7  
            group by s.summaryid ) ss
                where ss.interview_id  = interviewid");
            }
        }

        public override void Down()
        {
        }
    }
}
