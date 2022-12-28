using Android.App;
using Android.Content;
using Android.Content.PM;
using Android.OS;
using Android.Views;
using AndroidX.AppCompat.Widget;
using Java.Interop;
using WB.Core.BoundedContexts.Supervisor.ViewModel;
using WB.Core.SharedKernels.Enumerator.Properties;
using WB.UI.Shared.Enumerator.Activities;
using WB.UI.Shared.Enumerator.Activities.Callbacks;
using Toolbar=AndroidX.AppCompat.Widget.Toolbar;

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
            
            OnBackPressedDispatcher.AddCallback(this, new OnBackPressedCallbackWrapper(() =>
            {
                if (this.ViewModel.IsInProgress)
                {
                    this.ViewModel.CancelInProgressTask();
                }
            }));
        }

        public override bool OnCreateOptionsMenu(IMenu menu)
        {
            this.MenuInflater.Inflate(Resource.Menu.finish, menu);

            menu.LocalizeMenuItem(Resource.Id.menu_settings, EnumeratorUIResources.MenuItem_Title_Settings);

            menu.LocalizeMenuItem(Resource.Id.menu_diagnostics, EnumeratorUIResources.MenuItem_Title_Diagnostics);
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

        
        [Export("FinishInstallation")]
        public void FinishInstallation(string url, string username, string password)
        {
            this.ViewModel.Endpoint = url;
            this.ViewModel.UserName = username;
            this.ViewModel.Password = password;
            this.ViewModel.SignInCommand.ExecuteAsync();
        }
    }
}
