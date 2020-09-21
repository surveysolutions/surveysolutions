﻿using Android.App;
using Android.Views;
using WB.Core.BoundedContexts.Supervisor.ViewModel;
using WB.UI.Shared.Enumerator.OfflineSync.Activities;

namespace WB.UI.Supervisor.Activities
{
    [Activity(Label = "",
        Theme = "@style/GrayAppTheme",
        WindowSoftInputMode = SoftInput.StateHidden,
        HardwareAccelerated = true,
        ConfigurationChanges = Android.Content.PM.ConfigChanges.Orientation | Android.Content.PM.ConfigChanges.ScreenSize,
        Exported = false)]
    public class OfflineSupervisorSyncActitivy : GoogleApiConnectedActivity<SupervisorOfflineSyncViewModel>
    {
        protected override int ViewResourceId => Resource.Layout.offline_sync;
    }
}
