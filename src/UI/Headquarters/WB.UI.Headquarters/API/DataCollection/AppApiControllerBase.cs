using System;
using System.Web.Http;
using WB.Core.BoundedContexts.Headquarters.DataExport.Security;
using WB.Core.BoundedContexts.Headquarters.Views;
using WB.Core.Infrastructure.PlainStorage;


namespace WB.UI.Headquarters.API.DataCollection
{
    public class AppApiControllerBase : ApiController
    {
        private readonly IPlainKeyValueStorage<InterviewerSettings> settingsStorage;

        public AppApiControllerBase(IPlainKeyValueStorage<InterviewerSettings> settingsStorage)
        {
            this.settingsStorage = settingsStorage;
        }

        protected bool IsNeedUpdateAppBySettings(Version appVersion, Version hqVersion)
        {
            var interviewerSettings = settingsStorage.GetById(AppSetting.InterviewerSettings);
            var majorUpdatesToUpdate = interviewerSettings != null ? interviewerSettings.HowManyMajorReleaseDontNeedUpdate : InterviewerSettings.HowManyMajorReleaseDontNeedUpdateDefaultValue;
            if (majorUpdatesToUpdate.HasValue)
            {
                var acceptedInterviewerVersion = hqVersion.Minor - majorUpdatesToUpdate.Value;
                if (appVersion.Major == hqVersion.Major && appVersion.Minor < acceptedInterviewerVersion)
                {
                    return true;
                }

                if (appVersion.Major == hqVersion.Major - 1)
                {
                    var acceptedInterviewerVersionForOldMajorVersion = (hqVersion.Minor + 12) - majorUpdatesToUpdate.Value;
                    if (appVersion.Minor < acceptedInterviewerVersionForOldMajorVersion)
                    {
                        return true;
                    }
                }

            }

            return false;
        }
    }
}
