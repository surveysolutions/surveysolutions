![Link Text](https://build.mysurvey.solutions/app/rest/builds/buildType:(id:CI_Package)/statusIcon)
20.4 version
- headquarters.cloud.ini should be replaced with headquarters.cloud.vnext.ini
- ClientSecret of [ExternalStorages:OAuth2:Dropbox] should be updated in file headquarters.cloud.ini
- data-export-storages.html file was changed, so first of all should be updated demo server
- data-export-storages.html should be copied to Portal

20.3 version
- runs on .net core 3.1
- requires chaning of configuraiton file from xml to ini format
- link to old audit log that was in file is going to be removed
- initial setup should be made by executing following 2 commands:
`WB.UI.Headquarters.exe manage migrate`
`WB.UI.Headquarters.exe manage users create --role=Administrator --login=admin --password=P@$$w0rd`

[Release notes](https://github.com/surveysolutions/surveysolutions/wiki/Release-notes)
