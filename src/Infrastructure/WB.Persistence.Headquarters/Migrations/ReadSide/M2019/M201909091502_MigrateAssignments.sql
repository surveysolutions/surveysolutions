INSERT INTO readside.assignments (publickey, id, responsibleid, quantity, archived, createdatutc, updatedatutc, questionnaireid, questionnaireversion, answers, questionnaire, protectedvariables, receivedbytabletatutc, isaudiorecordingenabled, email, password, webmode)
SELECT NEWID(), id, responsibleid, quantity, archived, createdatutc, updatedatutc, questionnaireid, questionnaireversion, answers, questionnaire, protectedvariables, receivedbytabletatutc, isaudiorecordingenabled, email, password, webmode
	FROM plainstore.assignments;
	
INSERT INTO readside.assignmentsidentifyinganswers(assignmentid, "position", questionid, answer, answerasstring, rostervector)
SELECT assignmentid, "position", questionid, answer, answerasstring, rostervector
	FROM plainstore.assignmentsidentifyinganswers;
	
INSERT INTO events.events (id, origin, "timestamp", eventsourceid, value, eventsequence, eventtype)
SELECT NEWID(), -- id
	null,       -- origin
	createdatutc, -- "timestamp"
	publickey,  -- eventsourceid
	'{
		"id": ' + id + ',
		"userId": "00000000-0000-0000-0000-000000000001", -- admin id for first user on site
		"originDate": "' + createdatutc + '",
		"assignmentId": ' + id + ',
		"creationTime": "' + createdatutc + '",
		"questionnaireId": "' + questionnaireid + '",
		"questionnaireVersion": '+ questionnaireversion +',
		"isAudioRecordingEnabled": ' + isaudiorecordingenabled + '
	}',         -- value
	1,          -- eventsequence
	'AssignmentCreated' --eventtype
FROM readside.assignments;
