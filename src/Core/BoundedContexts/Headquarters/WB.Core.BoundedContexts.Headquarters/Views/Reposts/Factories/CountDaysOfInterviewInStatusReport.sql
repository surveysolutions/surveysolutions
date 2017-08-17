select status, timestamp as StatusDate, COUNT(*) as InterviewsCount from (
	SELECT isum.status, cast(timestamp as date), row_number() over (partition by ics."interviewid" order by position desc) rnk
		FROM readside.interviewcommentedstatuses ics
			INNER JOIN readside.interviewstatuses ist ON ics.interviewid = ist.id
			INNER JOIN readside.interviewsummaries isum ON CAST(ics.interviewid as uuid) = isum.interviewid
		WHERE 
		(
		   (isum.status = 100 /*Completed*/ AND ics.status = 3 /*Completed*/ )
		   OR (isum.status = 120 /*ApprovedBySupervisor*/ AND ics.status = 5 /* ApprovedBySupervisor */)
		   OR (isum.status = 65  /*RejectedBySupervisor*/ AND ics.status = 7 /* RejectedBySupervisor */)
		)
		AND (ist.questionnaireid = @questionnaireid OR @questionnaireid is NULL)
		AND (ist.questionnaireversion = @questionnaireversion OR @questionnaireversion is NULL)
	) tmp 
where rnk = 1
GROUP BY status, timestamp