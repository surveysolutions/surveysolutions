using Android.App;
using Android.Content;
using Android.Content.PM;
using Android.OS;
using Android.Support.V7.Widget;
using Android.Views;
using WB.Core.BoundedContexts.Supervisor.ViewModel;
using WB.Core.GenericSubdomains.Portable.Tasks;
using WB.Core.SharedKernels.Enumerator.Properties;
using WB.UI.Shared.Enumerator.Activities;

namespace WB.UI.Supervisor.Activities
{
    [Activity(WindowSoftInputMode = SoftInput.StateHidden, 
        HardwareAccelerated = true,
        Theme = "@style/GrayAppTheme", 
        ConfigurationChanges = ConfigChanges.Orientation | ConfigChanges.ScreenSize,
        Exported = false)]
    public class FinishInstallationActivity : BaseActivity<FinishInstallationViewModel>
    {
        protected override int ViewResourceId => Resource.Layout.finish_installation;

        protected override void OnCreate(Bundle bundle)
        {
            base.OnCreate(bundle);
            var toolbar = this.FindViewById<Toolbar>(Resource.Id.toolbar);
            toolbar.Title = "";
            this.SetSupportActionBar(toolbar);
        }

        public override void OnBackPressed()
        {
            if (this.ViewModel.IsInProgress)
            {
                this.ViewModel.CancellInProgressTask();
            }
        }

        public override bool OnCreateOptionsMenu(IMenu menu)
        {
            this.MenuInflater.Inflate(Resource.Menu.finish, menu);

            menu.LocalizeMenuItem(Resource.Id.menu_settings, InterviewerUIResources.MenuItem_Title_Settings);

            menu.LocalizeMenuItem(Resource.Id.menu_diagnostics, InterviewerUIResources.MenuItem_Title_Diagnostics);
            return base.OnCreateOptionsMenu(menu);
        }
        public override bool OnOptionsItemSelected(IMenuItem item)
        {
            switch (item.ItemId)
            {
                case Resource.Id.menu_settings:
                    Intent intent = new Intent(this, typeof(PrefsActivity));
                    this.StartActivity(intent);
                    break;
                case Resource.Id.menu_diagnostics:
                    this.ViewModel.NavigateToDiagnosticsPageCommand.Execute();
                    break;
            }
            return base.OnOptionsItemSelected(item);
        }
    }
}
