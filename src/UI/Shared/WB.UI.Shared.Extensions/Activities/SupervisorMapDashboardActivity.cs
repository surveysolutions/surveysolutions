using Android.Content.PM;
using Android.Views;
using WB.UI.Shared.Extensions.ViewModels;

namespace WB.UI.Shared.Extensions.Activities;

[Activity(WindowSoftInputMode = SoftInput.StateHidden,
    Theme = "@style/AppTheme",
    ConfigurationChanges = ConfigChanges.Orientation | ConfigChanges.ScreenSize,
    NoHistory = true,
    Exported = false)]
public class SupervisorMapDashboardActivity: MapDashboardActivity<SupervisorMapDashboardViewModel>
{
    
}
