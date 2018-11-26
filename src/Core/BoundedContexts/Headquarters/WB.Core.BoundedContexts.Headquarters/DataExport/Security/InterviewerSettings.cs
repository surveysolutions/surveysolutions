using WB.Core.BoundedContexts.Headquarters.Views;

namespace WB.Core.BoundedContexts.Headquarters.DataExport.Security
{
    public class InterviewerSettings : AppSetting
    {
        public const int HowManyMajorReleaseDontNeedUpdateDefaultValue = 4;

        public bool AutoUpdateEnabled { get; set; }
        public int? HowManyMajorReleaseDontNeedUpdate { get; set; } = HowManyMajorReleaseDontNeedUpdateDefaultValue;
    }
}
