using FluentMigrator;

namespace WB.Persistence.Headquarters.Migrations.ReadSide
{
    [Migration(202007141132)]
    public class M202007141132_AddedTabletReceivedDateTime : Migration
    {
        public override void Up()
        {
            Create.Column("receivedbyinterviewertabletatutc").OnTable("InterviewSummaries")
                .AsDateTime()
                .Nullable();
            
            Execute.Sql(@"update readside.interviewsummaries isum
                set isum.receivedbyinterviewertabletatutc = (
		        select e.""timestamp"" 
                    from events.events e 
                        where isum.interviewid = e.eventsourceid and e.eventtype = 'InterviewReceivedByInterviewer'
                    order by eventsequence desc
                    limit 1
                        ) 
                where isum.receivedbyinterviewer = true;");
            
            Delete.Column("receivedbyinterviewer").FromTable("InterviewSummaries");
        }

        public override void Down()
        {
            
        }
    }
}
