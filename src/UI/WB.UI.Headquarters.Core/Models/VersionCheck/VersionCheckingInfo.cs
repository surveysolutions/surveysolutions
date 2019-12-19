using WB.Core.BoundedContexts.Headquarters.DataExport.Security;
using WB.Core.BoundedContexts.Headquarters.Views;

namespace WB.UI.Headquarters.Models.VersionCheck
{
    public class VersionCheckingInfo : AppSetting
    {
        public int Build { set; get; }
        public string Version { set; get; }
        public string VersionString { set; get; }
    }
}