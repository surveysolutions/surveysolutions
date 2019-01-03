using System;
using System.Web.Http;
using WB.Core.BoundedContexts.Headquarters.DataExport.Security;
using WB.Core.BoundedContexts.Headquarters.Views;
using WB.Core.Infrastructure.PlainStorage;


namespace WB.UI.Headquarters.API.DataCollection
{
    public class AppApiControllerBase : ApiController
    {
        private readonly Version LastSupportedVersion = new Version(18, 04, 0, 0); // version from the sky, discussed on scrum 12/19/2018

        private readonly IPlainKeyValueStorage<InterviewerSettings> settingsStorage;

        public AppApiControllerBase(IPlainKeyValueStorage<InterviewerSettings> settingsStorage)
        {
            this.settingsStorage = settingsStorage;
        }

        protected bool IsNeedUpdateAppBySettings(Version appVersion, Version hqVersion)
        {
            if (appVersion == null)
                return false;

            var interviewerSettings = settingsStorage.GetById(AppSetting.InterviewerSettings);
            if (interviewerSettings.GetWithDefaultValue())
            {
                return hqVersion != appVersion;
            }

            return appVersion < LastSupportedVersion;
        }
    }
}
