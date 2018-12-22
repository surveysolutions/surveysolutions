using WB.Core.BoundedContexts.Headquarters.Views;

namespace WB.Core.BoundedContexts.Headquarters.DataExport.Security
{
    public class InterviewerSettings : AppSetting
    {
        public const bool AutoUpdateEnabledDefault = true;

        public bool AutoUpdateEnabled { get; set; } = AutoUpdateEnabledDefault;
    }
}
