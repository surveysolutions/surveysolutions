using WB.Core.BoundedContexts.Headquarters.Views;

namespace WB.Core.BoundedContexts.Headquarters.ValueObjects
{
    public class GlobalNotice : AppSetting
    {
        public string Message { get; set; }
    }
}
