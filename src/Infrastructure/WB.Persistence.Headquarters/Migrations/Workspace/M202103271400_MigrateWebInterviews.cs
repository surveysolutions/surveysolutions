using System.Diagnostics.CodeAnalysis;
using FluentMigrator;

namespace WB.Persistence.Headquarters.Migrations.Workspace
{
    [Migration(202103271400)]
    public class M202103271400_MigrateWebInterviews : ForwardOnlyMigration
    {
        [SuppressMessage("ReSharper", "StringLiteralTypo")]
        public override void Up()
        {
            Execute.Sql(@"DROP EXTENSION IF EXISTS ""uuid-ossp"";create extension if not exists ""uuid-ossp"" schema public;");
            
            Execute.Sql($@"INSERT INTO events
                (id, origin, ""timestamp"", eventsourceid, value, eventsequence, eventtype)
            select
                public.uuid_generate_v4() as id,
                'migration_to_web_interview' as origin,
                CURRENT_TIMESTAMP as ""timestamp"",
                i.interviewid as eventsourceid,
                json_build_object(
                    'mode', 'CAWI',
                    'originDate', to_char(now()::timestamp at time zone 'UTC', 'YYYY-MM-DD""T""HH24:MI:SS""Z""'),
                    'comment', 'Migration to new WebInterview status'
                ) as value,
                max(e.eventsequence) + 1 as eventsequence,
                'InterviewModeChanged' as eventtype
            from interviewsummaries i
                join assignments a on i.assignmentid = a.id
            join events e on i.interviewid = e.eventsourceid
            where i.status in (
                --InterviewerAssigned
                {(int)InterviewStatus.InterviewerAssigned},
                --RejectedBySupervisor =
                {(int)InterviewStatus.RejectedBySupervisor})
            group by i.interviewid, i.status");

            Execute.Sql($@"update interviewsummaries i
                set interview_mode = {(int)InterviewMode.CAWI}
                from assignments a 
                where i.assignmentid = a.id and 
                i.status in (
	                --InterviewerAssigned
                {(int)InterviewStatus.InterviewerAssigned},
                --RejectedBySupervisor =
                {(int)InterviewStatus.RejectedBySupervisor})");
        }

        enum InterviewMode
        {
            CAPI = 1,
            CAWI = 2
        }

        enum InterviewStatus
        {
            Deleted = -1,
            Restored = 0,

            Created = 20,
            SupervisorAssigned = 40,
            InterviewerAssigned = 60,
            ReadyForInterview = 80,
            SentToCapi = 85,
            Restarted = 95,
            Completed = 100,

            RejectedBySupervisor = 65,
            ApprovedBySupervisor = 120,

            RejectedByHeadquarters = 125,
            ApprovedByHeadquarters = 130
        }
    }
}
