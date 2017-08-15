select COUNT(DISTINCT up."SupervisorId")
from users.users u
	inner join users.userprofiles up on u."UserProfileId" = up."Id"
-- find last synchronization info
	LEFT JOIN 
	(
		SELECT dsi1."DeviceDate", dsi1."SyncDate", dsi1."AndroidSdkVersion", dsi1."InterviewerId", dsi1."StorageFreeInBytes"
		FROM plainstore.devicesyncinfo dsi1
		INNER JOIN (SELECT dsi."InterviewerId", MAX(dsi."Id") AS maxid
			    FROM plainstore.devicesyncinfo dsi GROUP BY dsi."InterviewerId") AS p2
		  ON (dsi1."Id" = p2.maxid)
		  LEFT JOIN  plainstore.devicesyncstatistics dss on dsi1."StatisticsId" = dss."Id"
		) AS lastSync ON lastSync."InterviewerId" = u."Id"::uuid 
WHERE u."UserName" ILIKE @filter