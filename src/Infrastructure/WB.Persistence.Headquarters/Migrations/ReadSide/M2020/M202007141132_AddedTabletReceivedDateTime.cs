using FluentMigrator;

namespace WB.Persistence.Headquarters.Migrations.ReadSide
{
    [Migration(202007141132)]
    public class M202007141132_AddedTabletReceivedDateTime : Migration
    {
        public override void Up()
        {
            Create.Column("receivedbyintervieweratutc").OnTable("interviewsummaries")
                .AsDateTime()
                .Nullable();
            
            if (this.Schema.Schema("events").Exists())
            {
                if (this.Schema.Schema("events").Table("events").Exists())
                {
                    Execute.Sql(@"update readside.interviewsummaries as isum
                set receivedbyintervieweratutc = (
		        select e.""timestamp"" 
                    from events.events e 
                        where isum.interviewid = e.eventsourceid and e.eventtype = 'InterviewReceivedByInterviewer'
                    order by eventsequence desc
                    limit 1
                        ) 
                where isum.receivedbyinterviewer = true;");
                }
            }

            Delete.Column("receivedbyinterviewer").FromTable("interviewsummaries");
        }

        public override void Down()
        {
            
        }
    }
}
