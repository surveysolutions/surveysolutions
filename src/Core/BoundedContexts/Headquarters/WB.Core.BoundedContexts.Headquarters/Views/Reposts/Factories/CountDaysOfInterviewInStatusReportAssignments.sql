select cast(CreatedDate as date), RoleId, SUM(Quantity - Count) as Count from (
	select 
		ass.CreatedAtUtc + (@minutesoffset ||' minutes')::interval as CreatedDate, 
		ur."RoleId" as RoleId,
		ass.Quantity as Quantity,
		(select count(*) from readside.InterviewSummaries isum where ass.Id=isum.assignmentid) as Count
	from plainstore.Assignments ass
		left outer join users.userroles ur on ass.ResponsibleId = ur."UserId"
	where ass.Quantity is not null 
		AND (ass.questionnaireid = @questionnaireid OR @questionnaireid is NULL)
		AND (ass.questionnaireversion = @questionnaireversion OR @questionnaireversion is NULL)
	) tmp
	where Quantity > Count
group by cast (CreatedDate as date), RoleId
--order by CreatedAtUtc
		