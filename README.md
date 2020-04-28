![Link Text](https://build.mysurvey.solutions/app/rest/builds/buildType:(id:CI_Package)/statusIcon)
20.4 version
- data-export-storages.html file was changed, so first of all should be updated demo server
- for Headquarters configuration should be made following changes:
[ExternalStorages:OAuth2]
ResponseType="code"

[ExternalStorages:OAuth2:Dropbox]
TokenUri ="https://api.dropbox.com/1/oauth2"
ClientSecret="client secret here"

[ExternalStorages:OAuth2:OneDrive]
TokenUri ="https://login.microsoftonline.com/common/oauth2/v2.0"
ClientSecret="client secret here"
Scope="files.readwrite.all offline_access"

[ExternalStorages:OAuth2:GoogleDrive]
TokenUri ="https://oauth2.googleapis.com"
ClientSecret="client secret here"

20.3 version
- runs on .net core 3.1
- requires chaning of configuraiton file from xml to ini format
- link to old audit log that was in file is going to be removed
- initial setup should be made by executing following 2 commands:
`WB.UI.Headquarters.exe manage migrate`
`WB.UI.Headquarters.exe manage users create --role=Administrator --login=admin --password=P@$$w0rd`

[Release notes](https://github.com/surveysolutions/surveysolutions/wiki/Release-notes)
