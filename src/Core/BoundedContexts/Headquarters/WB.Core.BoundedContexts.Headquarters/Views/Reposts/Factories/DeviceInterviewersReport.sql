select wu.supervisor_id TeamId, 
       userNames."UserName" TeamName,
       SUM(case when up."DeviceId" IS NULL then 1 else 0 end) NeverSynchedCount, 
       SUM(case when up."DeviceAppBuildVersion" IS NOT NULL AND up."DeviceAppBuildVersion" < @latestAppBuildVersion then 1 else 0 end) OutdatedCount, 
       SUM(case when lastSync."AndroidSdkVersion" < @targetAndroidSdkVersion then 1 else 0 end) OldAndroidCount,
       SUM(case when lastSync."StorageFreeInBytes" < @neededFreeStorageInBytes then 1 else 0 end) LowStorageCount, 
       SUM(case when anySync."InterviewerId" IS NULL then 1 else 0 end) NeverUploadedCount,
       SUM(case when wasReassign."InterviewerId" IS NOT NULL then 1 else 0 end) ReassignedCount,
       SUM(case when anySyncWithQuestionnaire."InterviewerId" IS NULL then 1 else 0 end) NoQuestionnairesCount,
       SUM(case when lastSync."DeviceDate" = '0001-01-01T00:00:00' OR 
           abs(extract(epoch from date_trunc('minute', lastSync."SyncDate") - date_trunc('minute', lastSync."DeviceDate"))) / 60 > @minutesMismatch then 1 else 0 end) WrongDateOnTabletCount,
   
       COUNT(up."Id") TeamSize
from users.users u
	inner join users.userprofiles up on u."UserProfileId" = up."Id"
    inner join workspaces.workspace_users wu on wu.user_id = u."Id" and wu.workspace = @workspace
-- find last synchronization info
	LEFT JOIN 
	(
		SELECT dsi1."DeviceDate", dsi1."SyncDate", dsi1."AndroidSdkVersion", dsi1."InterviewerId", dsi1."StorageFreeInBytes"
		FROM devicesyncinfo dsi1
		INNER JOIN (SELECT dsi."InterviewerId", MAX(dsi."Id") AS maxid
			    FROM devicesyncinfo dsi GROUP BY dsi."InterviewerId") AS p2
		  ON (dsi1."Id" = p2.maxid)
		  LEFT JOIN  devicesyncstatistics dss on dsi1."StatisticsId" = dss."Id"
		) AS lastSync ON lastSync."InterviewerId" = u."Id" 

-- find if there was a sync with > 0 uploaded interviews
LEFT JOIN (SELECT DISTINCT dsi."InterviewerId" FROM devicesyncinfo dsi WHERE EXISTS(SELECT 1 FROM devicesyncstatistics WHERE dsi."StatisticsId" = "Id" AND "UploadedInterviewsCount" > 0)) 
	AS anySync ON anySync."InterviewerId" = u."Id"

-- find how many interviewers have more than 1 tablet
LEFT JOIN (SELECT "InterviewerId" FROM devicesyncinfo dsi 
GROUP BY "InterviewerId"
HAVING COUNT(DISTINCT "DeviceId") > 1) AS wasReassign ON wasReassign."InterviewerId" = u."Id"

-- find if there was any questionnaire received
LEFT JOIN (SELECT DISTINCT dsi."InterviewerId" FROM devicesyncinfo dsi WHERE EXISTS(SELECT 1 FROM devicesyncstatistics WHERE dsi."StatisticsId" = "Id" AND "DownloadedQuestionnairesCount" > 0)) 
	AS anySyncWithQuestionnaire ON anySyncWithQuestionnaire."InterviewerId" = u."Id"

-- find supervisor name
INNER JOIN users.users userNames ON wu.supervisor_id = userNames."Id"

WHERE wu.supervisor_id IS NOT null AND u."IsArchived" = false AND userNames."UserName" ILIKE @filter
GROUP BY wu.supervisor_id, userNames."UserName"
ORDER BY {0}
LIMIT @limit OFFSET @offset
