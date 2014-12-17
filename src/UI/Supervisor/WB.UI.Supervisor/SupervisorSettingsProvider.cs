using System.Collections.Generic;
using System.Web.Configuration;
using WB.UI.Shared.Web.Settings;

namespace WB.UI.Supervisor
{
    public class SupervisorSettingsProvider : SettingsProvider, ISettingsProvider
    {
        public override IEnumerable<ApplicationSetting> GetSettings()
        {
            foreach (var applicationSetting in base.GetSettings())
            {
                yield return applicationSetting;
            }

            var hqSettings = (HeadquartersSettings) WebConfigurationManager.GetSection("headquartersSettingsGroup/headquartersSettings");

            yield return new ApplicationSetting("BaseHqUrl", hqSettings.BaseHqUrl);
            yield return new ApplicationSetting("InterviewsFeedUrl", hqSettings.InterviewsFeedUrl);
        }
    }
}