using Android.Content;
using Android.Views;
using WB.Core.SharedKernels.Enumerator.Services.MapSynchronization;
using WB.Core.SharedKernels.Enumerator.Services.Synchronization;
using WB.Core.SharedKernels.Enumerator.Views;
using WB.UI.Shared.Enumerator.Activities;
using WB.UI.Shared.Enumerator.Services;
using Toolbar=AndroidX.AppCompat.Widget.Toolbar;

namespace WB.UI.Interviewer.Activities
{
    [Activity(Theme = "@style/GrayAppTheme",
        WindowSoftInputMode = SoftInput.StateHidden,
        HardwareAccelerated = true,
        ConfigurationChanges = Android.Content.PM.ConfigChanges.Orientation | Android.Content.PM.ConfigChanges.ScreenSize,
        NoHistory = true,
        Exported = false)]
    public class MapsActivity : BaseActivity<MapsViewModel>, ISyncBgService<MapSyncProgressStatus>, ISyncServiceHost<MapDownloadBackgroundService>
    {
        private SyncServiceConnection<MapDownloadBackgroundService> connection;
        private bool isBound;
        public ServiceBinder<MapDownloadBackgroundService> Binder { get; set; }

        protected override int ViewResourceId => Resource.Layout.maps;

        protected override void OnCreate(Bundle bundle)
        {
            base.OnCreate(bundle);
            var toolbar = this.FindViewById<Toolbar>(Resource.Id.toolbar);
            toolbar.Title = "";
            this.SetSupportActionBar(toolbar);
            
            this.SupportActionBar.SetDisplayHomeAsUpEnabled(true);
        }

        public override bool OnSupportNavigateUp() {
            OnBackPressedDispatcher.OnBackPressed();
            return true;
        }

        protected override void OnViewModelSet()
        {
            base.OnViewModelSet();
            this.ViewModel.Synchronization.MapSyncBackgroundService = this;
        }

        protected override void OnStart()
        {
            base.OnStart();
            connection = new SyncServiceConnection<MapDownloadBackgroundService>(this);
            this.BindService(new Intent(this, typeof(MapDownloadBackgroundService)), connection, Bind.AutoCreate);
            isBound = true;
        }

        protected override void OnStop()
        {
            base.OnStop();
            
            if(!isBound) return;
            
            this.UnbindService(connection);
            isBound = false;
            connection = null;
        }

        public override bool OnCreateOptionsMenu(IMenu menu)
        {
            this.MenuInflater.Inflate(Resource.Menu.maps, menu);
            return base.OnCreateOptionsMenu(menu);
        }
        public override bool OnOptionsItemSelected(IMenuItem item)
        {
            switch (item.ItemId)
            {
                case Resource.Id.menu_map_synchronization:
                    this.ViewModel.MapSynchronizationCommand.Execute();
                    break;
            }
            return base.OnOptionsItemSelected(item);
        }
        
        public void StartSync() => this.Binder.GetService().SyncMaps();

        public MapSyncProgressStatus CurrentProgress => this.Binder.GetService().CurrentProgress;
    }
}
