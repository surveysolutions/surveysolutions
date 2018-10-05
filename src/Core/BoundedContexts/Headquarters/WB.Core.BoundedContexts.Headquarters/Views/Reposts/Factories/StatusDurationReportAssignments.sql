select Days, RoleId, SUM(Quantity - Count) as Count from (
	select  
        DATE_PART('day', (now() at time zone 'utc')::timestamp - ass.CreatedAtUtc::timestamp) + 1 as Days, 
		ur."RoleId" as RoleId,
		ass.Quantity as Quantity,
		(select count(*) from readside.InterviewSummaries isum where ass.Id=isum.assignmentid) as Count
	from plainstore.Assignments ass
		left outer join users.userroles ur on ass.ResponsibleId = ur."UserId"
		left outer join users.users us on us."Id" = ass.ResponsibleId
		left outer join users.userprofiles up on us."UserProfileId" = up."Id"
	where ass.Quantity is not null 
		AND ass.archived = false
		AND (ass.questionnaireid = @questionnaireid OR @questionnaireid is NULL)
		AND (ass.questionnaireversion = @questionnaireversion OR @questionnaireversion is NULL)
		AND (up."SupervisorId" = @supervisorid OR ass.ResponsibleId = @supervisorid OR @supervisorid is NULL)
	) tmp
	where Quantity > Count
group by Days, RoleId
--order by Days
