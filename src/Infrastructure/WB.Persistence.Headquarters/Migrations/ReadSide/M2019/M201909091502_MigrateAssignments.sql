INSERT INTO readside.assignments (publickey, id, responsibleid, quantity, archived, createdatutc, updatedatutc, questionnaireid, questionnaireversion, answers, questionnaire, protectedvariables, receivedbytabletatutc, audiorecording, email, password, webmode)
SELECT 
    uuid_in(md5(random()::text || clock_timestamp()::text)::cstring),
	id,
	responsibleid, 
	quantity, 
	archived, 
	createdatutc, 
	updatedatutc, 
	questionnaireid, 
	questionnaireversion, 
	answers, 
	questionnaire, 
	protectedvariables, 
	receivedbytabletatutc, 
	isaudiorecordingenabled, 
	email, 
	password, 
	webmode
FROM plainstore.assignments;
	
INSERT INTO readside.assignmentsidentifyinganswers(assignmentid, "position", questionid, answer, answerasstring, rostervector)
SELECT 
	ans.assignmentid, --assignmentid, 
	ans."position", 
	ans.questionid, 
	ans.answer, 
	ans.answerasstring, 
	ans.rostervector
FROM plainstore.assignmentsidentifyinganswers ans;
	
INSERT INTO events.events (id, origin, "timestamp", eventsourceid, eventsequence, eventtype, value)
SELECT 
	uuid_in(md5(random()::text || clock_timestamp()::text)::cstring),      -- id
	null,                -- origin
	createdatutc,        -- "timestamp"
	publickey,           -- eventsourceid
	1,                   -- eventsequence
	'AssignmentCreated', -- eventtype
	concat('{ 
		   "id": ', id, ', 
		   "userId": "', (SELECT "UserId"::text FROM users.userroles r inner join users.users u on r."UserId"=u."Id" where "RoleId"='00000000-0000-0000-0000-000000000001' limit 1), '", 
		   "answers": ', coalesce(answers::text, '[]'), ', 
		   "webMode": ', coalesce(webmode::text, 'null'), ',
		   "quantity": ', coalesce(quantity::text, 'null'), ',
		   "originDate": "', createdatutc, '",
		   "responsibleId": "', responsibleId, '",
		   "protectedVariables": ', coalesce(protectedVariables::text, '[]'), ',
		   "questionnaireId": "', questionnaireid, '",
		   "questionnaireVersion": ', questionnaireversion, ',
		   "audioRecording": ', audiorecording::text, '
	}')::jsonb          -- value
FROM readside.assignments;


INSERT INTO events.events (id, origin, "timestamp", eventsourceid, eventsequence, eventtype, value)
SELECT 
	uuid_in(md5(random()::text || clock_timestamp()::text)::cstring),      -- id
	null,                -- origin
	updatedatutc,        -- "timestamp"
	publickey,           -- eventsourceid
	(SELECT MAX(eventsequence) + 1 FROM events.events es WHERE es.eventsourceid = publickey),             -- eventsequence
	'AssignmentReceivedByTablet', -- eventtype
	concat('{ 
		   "originDate": "', updatedatutc, '",
		   "userId": "', (SELECT "UserId"::text FROM users.userroles r inner join users.users u on r."UserId"=u."Id" where "RoleId"='00000000-0000-0000-0000-000000000001' limit 1), '"
	}')::jsonb          -- value
FROM readside.assignments  WHERE receivedbytabletatutc is not null;

INSERT INTO events.events (id, origin, "timestamp", eventsourceid, eventsequence, eventtype, value)
SELECT 
	uuid_in(md5(random()::text || clock_timestamp()::text)::cstring),      -- id
	null,                -- origin
	updatedatutc,        -- "timestamp"
	publickey,           -- eventsourceid
	(SELECT MAX(eventsequence) + 1 FROM events.events es WHERE es.eventsourceid = publickey),             -- eventsequence
	'AssignmentArchived', -- eventtype
	concat('{ 
		   "originDate": "', updatedatutc, '",
		   "userId": "', (SELECT "UserId"::text FROM users.userroles r inner join users.users u on r."UserId"=u."Id" where "RoleId"='00000000-0000-0000-0000-000000000001' limit 1), '"
	}')::jsonb          -- value
FROM readside.assignments WHERE archived = true;


