using FluentMigrator;

namespace WB.Persistence.Headquarters.Migrations.Workspace
{
    [Migration(202103052300)]
    public class M202103052300_MigrateWebInterviews : ForwardOnlyMigration
    {
        public override void Up()
        {
            Execute.Sql(@"DROP EXTENSION IF EXISTS ""uuid-ossp"";create extension if not exists ""uuid-ossp"" schema public;");
            Execute.Sql(@"
INSERT INTO events
(id, origin, ""timestamp"", eventsourceid, value, eventtype, eventsequence)
select
    public.uuid_generate_v4() as id,
    'migration_to_web_interview' as origin,
    CURRENT_TIMESTAMP as ""timestamp"",
    i.interviewid as eventsourceid,

    --{ ""status"": ""SupervisorAssigned"", ""utcTime"": ""2021 -01-25T11:55:51.9799693Z"", ""originDate"": ""2021 -01-25T13:55:51.9799693+02:00""}
            json_build_object(
                'status', 'WebInterview',
                'utcTime', to_char(now()::timestamp at time zone 'UTC', 'YYYY-MM-DD""T""HH24:MI:SS""Z""'),
                'originDate', to_char(now()::timestamp at time zone 'UTC', 'YYYY-MM-DD""T""HH24:MI:SS""Z""'),
                'comment', 'Migration to new WebInterview status',
                'previousStatus', case 
                    when i.status = 60 then 'InterviewerAssigned'        
                    when i.status = 65 then 'RejectedBySupervisor'        
                end
            ) as value,
		
	'InterviewStatusChanged' as eventtype,
	max(e.eventsequence) + 1 as eventsequence

from interviewsummaries i
join assignments a on i.assignmentid = a.id
join events e on i.interviewid = e.eventsourceid
where i.status in (
    --InterviewerAssigned
    60,
	--RejectedBySupervisor
    65) and a.webmode
group by i.interviewid, i.status");

            Execute.Sql(@"update interviewsummaries i
set status = 140 -- WebInterview
from assignments a 
where i.assignmentid = a.id and 
i.status in (
	--InterviewerAssigned
	60,
	--RejectedBySupervisor = 
	65) and a.webmode");
        }
    }
}
