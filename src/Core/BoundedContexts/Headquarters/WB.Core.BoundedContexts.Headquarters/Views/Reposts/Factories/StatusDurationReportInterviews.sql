select status, Days, COUNT(*) as InterviewsCount from (
	SELECT isum.status, 
		DATE_PART('day', (now() at time zone 'utc')::timestamp - timestamp::timestamp) + 1 as Days, 
		row_number() over (partition by ics."interviewid" order by position desc) rnk
	FROM readside.interviewcommentedstatuses ics
		INNER JOIN readside.interviewsummaries isum ON CAST(ics.interviewid as uuid) = isum.interviewid
	WHERE 
		(
		   (isum.status = 100 /*Completed*/ AND ics.status = 3 /*Completed*/ )
		   OR (isum.status = 120 /*ApprovedBySupervisor*/ AND ics.status = 5 /* ApprovedBySupervisor */)
		   OR (isum.status = 65  /*RejectedBySupervisor*/ AND ics.status = 7 /* RejectedBySupervisor */)
		   OR (isum.status = 125 /*RejectedByHeadquarters*/ AND ics.status = 8 /* RejectedByHeadquarters */)
		   OR (isum.status = 130 /*ApprovedByHeadquarters*/ AND ics.status = 6 /* ApprovedByHeadquarters */)
		)
		AND (isum.questionnaireid = @questionnaireid OR @questionnaireid is NULL)
		AND (isum.questionnaireversion = @questionnaireversion OR @questionnaireversion is NULL)
		AND (isum.teamleadid = @supervisorid OR @supervisorid is NULL)
	) tmp 
where rnk = 1
GROUP BY status, Days
--order by Days
