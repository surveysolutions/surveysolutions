using System;
using Microsoft.AspNetCore.Mvc;
using WB.Core.BoundedContexts.Headquarters.DataExport.Security;
using WB.Core.BoundedContexts.Headquarters.Implementation;
using WB.Core.BoundedContexts.Headquarters.Views;
using WB.Core.Infrastructure.PlainStorage;

namespace WB.UI.Headquarters.Controllers.Api.DataCollection
{
    public class AppControllerBaseBase : ControllerBase
    {
        private readonly Version LastSupportedVersion = new Version(19, 08, 0, 0); // version from the sky, discussed on scrum 12/04/2019

        private readonly IPlainKeyValueStorage<InterviewerSettings> settingsStorage;
        private readonly IPlainKeyValueStorage<TenantSettings> tenantSettings;

        public AppControllerBaseBase(IPlainKeyValueStorage<InterviewerSettings> settingsStorage, 
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
