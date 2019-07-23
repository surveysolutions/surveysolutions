using FluentMigrator;

namespace WB.Persistence.Headquarters.Migrations.ReadSide
{
    [Migration(201907171600)]
    public class M201907171600_FixSpeedReport : Migration
    {
        public override void Up()
        {
            Execute.Sql(@"with _affected as (
	            select id
	            from readside.interviewsummaries
	            where firstanswerdate is null
            ), 
             _statuses as 
	            (
		            select ics.interview_id, ics.interviewerid, ics.interviewername, ics.supervisorid, ics.supervisorname, ics.""timestamp"",
			            row_number() over (partition by ics.interview_id order by ""position"") as row_number
		            from readside.interviewcommentedstatuses ics
		            join _affected af on af.id = ics.interview_id
		            where interviewerid is not null and supervisorid is not null
	            ),
            _partialStatuses as (
		            select ics.interview_id, ics.interviewerid, ics.interviewername, ics.supervisorid, ics.supervisorname, ics.""timestamp"",
			            row_number() over (partition by ics.interview_id order by ""position"") as row_number
		            from readside.interviewcommentedstatuses ics
		            join _affected af on af.id = ics.interview_id
		            where supervisorid is not null and not exists (select 1 from _statuses s where s.interview_id = ics.interview_id)
            ),
            _upd as (select * from _statuses where row_number = 1 union select * from _partialStatuses where row_number = 1)
            update readside.interviewsummaries
            set 
	            firstinterviewerid = u.interviewerid,
	            firstinterviewername = u.interviewername,
	            firstsupervisorid = u.supervisorid,
	            firstsupervisorname = u.supervisorname,
	            firstanswerdate = u.""timestamp""
            from _upd u
            where u.interview_id = readside.interviewsummaries.id");
        }

        public override void Down()
        {
        }
    }
}
