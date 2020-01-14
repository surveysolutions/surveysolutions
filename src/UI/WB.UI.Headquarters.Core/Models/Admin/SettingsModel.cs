using System.Collections.Generic;
using WB.Core.BoundedContexts.Headquarters.DataExport;
using WB.Core.BoundedContexts.Headquarters.Implementation;

namespace WB.UI.Headquarters.Models.Admin
{
    public class SettingsModel
    {
        public IEnumerable<ApplicationSetting> Settings { get; set; }

        public ExportServiceSettings ExportSettings { get; set; }
    }
}
