using FluentMigrator;

namespace WB.Persistence.Headquarters.Migrations.ReadSide
{
    [Migration(201904011100)]
    public class M201904011100_MoveTimespanBetweenStatusesToInterviewId : Migration
    {
        public override void Up()
        {
            Execute.Sql(@"ALTER TABLE readside.timespanbetweenstatuses ADD interview_id int4 NULL;

                CREATE INDEX timespanbetweenstatuses_interview_idx ON readside.timespanbetweenstatuses (interviewid);

                with mapped as (select t.id, replace(e.eventsourceid::text, '-', '') as eventsourceid
                 from 
 	                readside.timespanbetweenstatuses as t inner join events.events as e 
 		                on date_trunc('seconds', t.endstatustimestamp) = date_trunc('seconds', (value ->> 'completeTime')::timestamp)
 			                and (e.value->>'userId')::uuid = t.interviewerid
                 where t.interviewid is null)

                update  readside.timespanbetweenstatuses s
                set interviewid = eventsourceid
                from mapped m where m.id = s.id;

                update readside.timespanbetweenstatuses c 
	                set interview_id = s.id
                from readside.interviewsummaries s
                where s.summaryid = c.interviewid;

                ALTER TABLE readside.timespanbetweenstatuses DROP COLUMN interviewid;

                ALTER TABLE readside.timespanbetweenstatuses ALTER COLUMN interview_id SET NOT NULL;

                CREATE INDEX timespanbetweenstatuses_interviewid_idx ON readside.timespanbetweenstatuses (interview_id);");
        }

        public override void Down()
        {
        }
    }
}
