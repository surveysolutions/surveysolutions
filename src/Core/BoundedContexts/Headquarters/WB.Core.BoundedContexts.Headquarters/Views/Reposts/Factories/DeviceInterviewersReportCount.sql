select COUNT(DISTINCT up."SupervisorId")
from users.userprofiles up
	inner join users.users u on up."SupervisorId" = u."Id"
WHERE u."UserName" ILIKE @filter