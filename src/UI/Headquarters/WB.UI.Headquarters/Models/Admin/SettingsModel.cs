using System.Collections.Generic;
using WB.Core.BoundedContexts.Headquarters.Implementation;
using WB.UI.Headquarters.API.Filters;
using WB.UI.Shared.Web.Settings;

namespace WB.UI.Headquarters.Models.Admin
{
    public class SettingsModel
    {
        public IEnumerable<ApplicationSetting> Settings { get; set; }

        public ExportServiceSettings ExportSettings { get; set; }
    }
}
