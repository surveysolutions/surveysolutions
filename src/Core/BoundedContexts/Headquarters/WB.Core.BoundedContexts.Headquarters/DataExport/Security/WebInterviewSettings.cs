using WB.Core.BoundedContexts.Headquarters.Views;

namespace WB.Core.BoundedContexts.Headquarters.DataExport.Security
{
    public class WebInterviewSettings : AppSetting
    {
        public const bool AllowEmailsDefault = false;
        
        public bool? AllowEmails { get; set; }
    }
}
