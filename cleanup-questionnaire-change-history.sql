DO language plpgsql $$
BEGIN
	UPDATE plainstore.questionnairechangerecords as changerecords
	SET resultingquestionnairedocument=null
	WHERE changerecords.sequence <= 
		(SELECT MAX(temprecords.sequence) - 500
		FROM plainstore.questionnairechangerecords as temprecords
		WHERE temprecords.questionnaireid = changerecords.questionnaireid);
END
$$;