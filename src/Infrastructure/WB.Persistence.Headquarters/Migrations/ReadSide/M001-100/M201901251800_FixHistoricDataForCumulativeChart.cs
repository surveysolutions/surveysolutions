using Dapper;
using FluentMigrator;
using Microsoft.Extensions.Logging;

namespace WB.Persistence.Headquarters.Migrations.ReadSide
{
    [Migration(201901251801)]
    public class M201901251801_FixHistoricDataForCumulativeChart : Migration
    {
        private readonly ILogger logger;

        public M201901251801_FixHistoricDataForCumulativeChart(ILoggerProvider logger)
        {
            this.logger = logger.CreateLogger(GetType().Name);
        }

        public override void Up()
        {
            this.Execute.WithConnection((d, t) =>
            {
                var eventsExists = !string.IsNullOrWhiteSpace(d.QuerySingle<string>(
                    "SELECT 'events.events'::regclass::text"));

                var cumulativeExists = !string.IsNullOrWhiteSpace(d.QuerySingle<string>(
                    "SELECT 'readside.cumulativereportstatuschanges'::regclass::text"));

                bool canProcessHistory = eventsExists && cumulativeExists;
                if (!canProcessHistory) return;

                void Execute(string sql)
                {
                    using (logger.BeginScope("Executing sql"))
                    {
                        logger.LogInformation(sql);
                        d.Execute(sql);
                    }
                }

                Execute(@"delete from readside.cumulativereportstatuschanges");
                Execute(@"
            insert into readside.cumulativereportstatuschanges
            with status_changes as (
	            select st.id, st.interviewid, st.eventsequence, st.date::date::timestamp, lag(st.status) over w as prev_status, status
	            from (
		            select id as id, eventsourceid as interviewid, eventsequence, timestamp as date,
		            case 
			            when value ->> 'status' = 'Deleted' then -1 
			            when value ->> 'status' = 'Restored' then 0 
			            when value ->> 'status' = 'Created' then 20 
			            when value ->> 'status' = 'SupervisorAssigned' then 40 
			            when value ->> 'status' = 'InterviewerAssigned' then 60 
			            when value ->> 'status' = 'ReadyForInterview' then 80 
			            when value ->> 'status' = 'SentToCapi' then 85
			            when value ->> 'status' = 'Restarted' then 95
			            when value ->> 'status' = 'Completed' then 100
			            when value ->> 'status' = 'RejectedBySupervisor' then 65
			            when value ->> 'status' = 'ApprovedBySupervisor' then 120
			            when value ->> 'status' = 'RejectedByHeadquarters' then 125
			            when value ->> 'status' = 'ApprovedByHeadquarters' then 130
		            end as status
		            from events.events 
		            where eventtype = 'InterviewStatusChanged' 
		            order by globalsequence
	            ) as st
	            window w as (partition by st.interviewid order by eventsequence)
            ),
            existing_data_chart as (
	            select replace(s.id::text, '-', '') as id, s.date, s.status, s.prev_status, s.interviewid, s.eventsequence, summ.questionnaireidentity
	            from status_changes s
	            inner join readside.interviewsummaries summ on summ.interviewid = s.interviewid
            ),
            cumulative_chart as (
	            select id || '-plus' as entryid, date, status, 1 as changevalue, interviewid, questionnaireidentity, eventsequence
	            from existing_data_chart
	            union 
	            select id || '-minus' as entryid, date, prev_status, -1 as changevalue, interviewid, questionnaireidentity, eventsequence
	            from existing_data_chart
	            where prev_status is not null
            )
            select entryid, date, status, changevalue, questionnaireidentity, interviewid, eventsequence
            from cumulative_chart");
            });
        }

        public override void Down()
        {

        }
    }
}
