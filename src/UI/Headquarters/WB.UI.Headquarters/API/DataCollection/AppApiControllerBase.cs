using System;
using System.Web.Http;
using WB.Core.BoundedContexts.Headquarters.DataExport.Security;
using WB.Core.BoundedContexts.Headquarters.Implementation;
using WB.Core.BoundedContexts.Headquarters.Views;
using WB.Core.Infrastructure.PlainStorage;


namespace WB.UI.Headquarters.API.DataCollection
{
    public class AppApiControllerBase : ApiController
    {
        // version from the sky, discussed on scrum 12/04/2019
        //revision is used to compare version of client apk
        private readonly Version LastSupportedVersion = new Version(19, 08, 0, 25531); 

        private readonly IPlainKeyValueStorage<InterviewerSettings> settingsStorage;
        private readonly IPlainKeyValueStorage<TenantSettings> tenantSettings;

        public AppApiControllerBase(IPlainKeyValueStorage<InterviewerSettings> settingsStorage, 
            IPlainKeyValueStorage<TenantSettings> tenantSettings)
        {
            this.settingsStorage = settingsStorage ?? throw new ArgumentNullException(nameof(settingsStorage));
            this.tenantSettings = tenantSettings ?? throw new ArgumentNullException(nameof(tenantSettings));
        }

        protected bool IsNeedUpdateAppBySettings(Version appVersion, Version hqVersion)
        {
            if (appVersion == null)
                return false;

            var interviewerSettings = settingsStorage.GetById(AppSetting.InterviewerSettings);
            if (interviewerSettings.IsAutoUpdateEnabled())
            {
                return hqVersion != appVersion;
            }

            return appVersion < LastSupportedVersion;
        }

        protected bool IsNeedUpdateAppBySettings(int? clientApkBuildNumber, int? serverApkBuildNumber)
        {
            if (clientApkBuildNumber == null)
                return false;

            var interviewerSettings = settingsStorage.GetById(AppSetting.InterviewerSettings);
            if (interviewerSettings.IsAutoUpdateEnabled())
            {
                return clientApkBuildNumber != serverApkBuildNumber;
            }

            return clientApkBuildNumber < LastSupportedVersion.Revision;
        }

        protected bool UserIsFromThisTenant(string userTenantId)
        {
            if (!string.IsNullOrEmpty(userTenantId))
            {
                var serverTenantId = this.tenantSettings.GetById(AppSetting.TenantSettingsKey).TenantPublicId;
                if (!userTenantId.Equals(serverTenantId, StringComparison.Ordinal))
                {
                    // https://httpstatuses.com/421
                    return false;
                }
            }

            return true;
        }
    }
}
