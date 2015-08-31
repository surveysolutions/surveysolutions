using Android.App;
using Android.OS;
using Android.Support.V7.Widget;
using Android.Views;
using WB.UI.Capi.ViewModel;
using WB.UI.Shared.Enumerator.Activities;

namespace WB.UI.Capi.Activities
{
    [Activity(WindowSoftInputMode = SoftInput.StateHidden, Theme = "@style/GrayAppTheme")]
    public class LoginActivity : BaseActivity<LoginActivityViewModel>
    {
        protected override int ViewResourceId
        {
            get { return Resource.Layout.Login; }
        }

        protected override void OnCreate(Bundle bundle)
        {
            base.OnCreate(bundle);
            var toolbar = this.FindViewById<Toolbar>(Resource.Id.toolbar);
            toolbar.Title = "";
            this.SetSupportActionBar(toolbar);
        }

        public override bool OnCreateOptionsMenu(IMenu menu)
        {
            this.MenuInflater.Inflate(Resource.Menu.login, menu);
            return base.OnCreateOptionsMenu(menu);
        }
        public override bool OnOptionsItemSelected(IMenuItem item)
        {
            switch (item.ItemId)
            {
                case Resource.Id.menu_settings:
                    this.ViewModel.NavigateToSettingsCommand.Execute();
                    break;
                case Resource.Id.menu_synchronization:
                    this.ViewModel.NavigateToSynchronizationCommand.Execute();
                    break;
            }
            return base.OnOptionsItemSelected(item);
        }
    }
}