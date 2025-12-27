using WB.Core.BoundedContexts.Designer.Views;

namespace WB.Core.BoundedContexts.Designer.Implementation;

public class AssistantSettings : AppSetting
{
    public bool IsEnabled { get; set; }
    public bool IsAvailableToAllUsers { get; set; }
}
