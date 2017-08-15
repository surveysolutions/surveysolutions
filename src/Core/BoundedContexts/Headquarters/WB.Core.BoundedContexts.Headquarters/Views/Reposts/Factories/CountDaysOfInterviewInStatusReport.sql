select status, timestamp as StatusDate, COUNT(*) as InterviewsCount from (
	SELECT status, cast(timestamp as date), row_number() over (partition by "interviewid" order by position desc) rnk
		FROM readside.interviewcommentedstatuses ics
			INNER JOIN readside.interviewstatuses ist ON ics.interviewid = ist.id
		WHERE (ist.questionnaireid = @questionnaireid OR @questionnaireid is NULL)
		  AND (ist.questionnaireversion = @questionnaireversion OR @questionnaireversion is NULL)
	) tmp 
where rnk = 1
GROUP BY status, timestamp