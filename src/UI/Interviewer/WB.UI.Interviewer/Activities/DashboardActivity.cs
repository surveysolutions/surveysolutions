using Android.App;
using Android.Content;
using Android.OS;
using Android.Support.V7.Widget;
using Android.Views;
using Cirrious.CrossCore;
using Cirrious.MvvmCross.Binding.Droid.BindingContext;
using Cirrious.MvvmCross.Droid.Support.RecyclerView;
using MvvmCross.Plugins.Messenger;
using WB.Core.BoundedContexts.Interviewer.Properties;
using WB.Core.BoundedContexts.Interviewer.Views;
using WB.Core.BoundedContexts.Interviewer.Views.Dashboard;
using WB.UI.Interviewer.CustomControls;
using WB.UI.Shared.Enumerator.Activities;

namespace WB.UI.Interviewer.Activities
{
    [Activity(Label = "", 
        Theme = "@style/GrayAppTheme", 
        WindowSoftInputMode = SoftInput.StateHidden,
        ConfigurationChanges = Android.Content.PM.ConfigChanges.Orientation | Android.Content.PM.ConfigChanges.ScreenSize)]
    public class DashboardActivity : BaseActivity<DashboardViewModel>
    {
        private readonly IMvxMessenger messenger = Mvx.Resolve<IMvxMessenger>();
        private MvxSubscriptionToken syncStartSubscriptionToken;
        private MvxSubscriptionToken syncEndSubscriptionToken;

        protected override int ViewResourceId
        {
            get { return Resource.Layout.dashboard; }
        }

        protected override void OnCreate(Bundle bundle)
        {
            base.OnCreate(bundle);

            this.syncStartSubscriptionToken = this.messenger.Subscribe<SyncronizationStartedMessage>(this.OnSyncStart);
            this.syncEndSubscriptionToken = this.messenger.Subscribe<SyncronizationStoppedMessage>(this.OnSyncStop);

            this.SetSupportActionBar(this.FindViewById<Toolbar>(Resource.Id.toolbar));

            var recyclerView = this.FindViewById<MvxRecyclerView>(Resource.Id.interviewsList);
            var layoutManager = new LinearLayoutManager(this);
            recyclerView.SetLayoutManager(layoutManager);
            recyclerView.HasFixedSize = true;

            var adapter = new InterviewerDashboardAdapter((IMvxAndroidBindingContext)this.BindingContext);
            recyclerView.Adapter = adapter;
        }

        private void OnSyncStop(SyncronizationStoppedMessage obj)
        {
            Window.ClearFlags(WindowManagerFlags.KeepScreenOn);
        }

        private void OnSyncStart(SyncronizationStartedMessage obj)
        {
            Window.AddFlags(WindowManagerFlags.KeepScreenOn);
        }

        public override void OnBackPressed()
        {
            
        }

        protected override void OnStop()
        {
            base.OnStop();

            if (this.syncEndSubscriptionToken != null)
            {
                this.messenger.Unsubscribe<SyncronizationStoppedMessage>(this.syncEndSubscriptionToken);
            }
            if (this.syncStartSubscriptionToken != null)
            {
                this.messenger.Unsubscribe<SyncronizationStartedMessage>(this.syncStartSubscriptionToken);
            }

            if (this.ViewModel != null)
                this.ViewModel.Synchronization.CancelSynchronizationCommand.Execute();
        }

        public override bool OnCreateOptionsMenu(IMenu menu)
        {
            this.MenuInflater.Inflate(Resource.Menu.dashboard, menu);

            menu.LocalizeMenuItem(Resource.Id.menu_signout, InterviewerUIResources.MenuItem_Title_SignOut);
            menu.LocalizeMenuItem(Resource.Id.menu_settings, InterviewerUIResources.MenuItem_Title_Settings);
            menu.LocalizeMenuItem(Resource.Id.menu_diagnostics, InterviewerUIResources.MenuItem_Title_Diagnostics);
            return base.OnCreateOptionsMenu(menu);
        }

        public override bool OnOptionsItemSelected(IMenuItem item)
        {
            switch (item.ItemId)
            {
                case Resource.Id.menu_synchronization:
                    this.ViewModel.SynchronizationCommand.Execute();
                    break;
                case Resource.Id.menu_settings:
                    Intent intent = new Intent(this, typeof(PrefsActivity));
                    this.StartActivity(intent);
                    break;
                case Resource.Id.menu_diagnostics:
                    this.ViewModel.NavigateToDiagnosticsPageCommand.Execute();
                    break;
                case Resource.Id.menu_signout:
                    this.ViewModel.SignOutCommand.Execute();
                    break;

            }
            return base.OnOptionsItemSelected(item);
        }
    }
}