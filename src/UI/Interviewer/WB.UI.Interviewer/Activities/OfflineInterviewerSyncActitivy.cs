using Android.App;
using Android.Views;
using WB.Core.BoundedContexts.Interviewer.Views;
using WB.UI.Shared.Enumerator.OfflineSync.Activities;

namespace WB.UI.Interviewer.Activities
{
    [Activity(Label = "",
        Theme = "@style/GrayAppTheme",
        WindowSoftInputMode = SoftInput.StateHidden,
        HardwareAccelerated = true,
        ConfigurationChanges = Android.Content.PM.ConfigChanges.Orientation | Android.Content.PM.ConfigChanges.ScreenSize)]
    public class OfflineInterviewerSyncActitivy : GoogleApiConnectedActivity<OfflineInterviewerSyncViewModel>
    {
        protected override int ViewResourceId => Resource.Layout.offline_sync;
    }
}
