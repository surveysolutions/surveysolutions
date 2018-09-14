# 18.10
- Manual step is required for Designer release. In order to shrink questionnaire history tool should be used
> ShrinkQuestionnaireHistory "%Connection string to database"

![Link Text](http://build.mysurvey.solutions/app/rest/builds/buildType:`(id:CI)`/statusIcon)
# 18.08
- For all HQs should be cleared PreLoadingTemplates folder
# 5.27
- All HQs which should have ability to export binary to external storages should get following code to web.config file to configuration section:
<configSections>
        <section name="externalStorages" type="WB.UI.Shared.Web.Configuration.ExternalStoragesConfigSection, WB.UI.Shared.Web, Version=5.22.20.0, Culture=neutral" />
    </configSections>
    <externalStorages>
        <oauth2 redirectUri="https://demo.mysurvey.solutions/data-export-storages.html" responseType="token">
            <dropbox authorizationUri="https://www.dropbox.com/1/oauth2/authorize" clientId="9uz71ejx33noq9a"></dropbox>
            <onedrive authorizationUri="https://login.live.com/oauth20_authorize.srf" clientId="964a0fcb-7b2e-47f0-a5bd-96e775fbb11c" scope="onedrive.readwrite"></onedrive>
            <googledrive authorizationUri="https://accounts.google.com/o/oauth2/v2/auth" clientId="571731722180-rqf3v08fc95mo1ccqg5oqrehrau32oh1.apps.googleusercontent.com" scope="https://www.googleapis.com/auth/drive.file"></googledrive>
        </oauth2>
    </externalStorages>
	
- After deployment of relase there is a script that should be executed from Gateway or Build server to sync images/export data from disk to s3 storage - [deploy-tools/utils/sync-all-files-to-s3.ps1](https://bitbucket.org/wbcapi/deploy-tools/src/master/utils/sync-all-files-to-s3.ps1?at=master&fileviewer=file-view-default)
# 5.26
- .net framework 4.7.1 is required
# 5.19
- Interviewer app lower than `5.8` version won't be able to synchronize anymore
- HQ apps with version lower than `5.8` won't be able to import questionnaires from designer
- Main pg connection string now requires additional argument: providerName="Npgsql"
# 5.18
IE issue: GPS question cannot be answered 2 times in 1 minute http://issues.mysurvey.solutions/youtrack/issue/KP-8547
# 5.17
- Web sockets windows feature should be enabled
