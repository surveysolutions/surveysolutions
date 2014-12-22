using System.Collections.Generic;

namespace WB.UI.Shared.Web.Settings
{
    public interface ISettingsProvider
    {
        IEnumerable<ApplicationSetting> GetSettings();
    }
}