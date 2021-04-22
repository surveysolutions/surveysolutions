CREATE VIEW user_profiles AS  
SELECT up."Id" as id, 
       up."DeviceId" as device_id, 
       ws.supervisor_id as supervisor_id, 
       up."DeviceAppVersion" as device_app_version, 
       up."DeviceAppBuildVersion" as device_app_build_version, 
       up."DeviceRegistrationDate" as device_registration_date, 
       up."StorageFreeInBytes" as storage_free_in_bytes 
FROM users.userprofiles up 
INNER JOIN users.users uu ON uu."UserProfileId" = up."Id" 
LEFT JOIN workspaces.workspace_users ws ON uu."Id" = ws.user_id AND ws.workspace = '$(workspace)'
