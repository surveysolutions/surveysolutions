![Link Text](https://build.mysurvey.solutions/app/rest/builds/buildType:(id:CI_Package)/statusIcon)

20.05 version 
** Not compatible ** with export service 20.04. Both should be in sync

20.3 version
- runs on .net core 3.1
- requires chaning of configuraiton file from xml to ini format
- link to old audit log that was in file is going to be removed
- initial setup should be made by executing following 2 commands:
`WB.UI.Headquarters.exe manage migrate`
`WB.UI.Headquarters.exe manage users create --role=Administrator --login=admin --password=P@$$w0rd`

[Release notes](https://github.com/surveysolutions/surveysolutions/wiki/Release-notes)
