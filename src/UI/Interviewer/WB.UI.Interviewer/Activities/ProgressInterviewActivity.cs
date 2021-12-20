using Android.Content;
using Android.OS;
using Android.Views;
using AndroidX.AppCompat.Widget;
using WB.Core.SharedKernels.Enumerator.Properties;
using WB.Core.SharedKernels.Enumerator.ViewModels;
using WB.UI.Shared.Enumerator.Activities;

namespace WB.UI.Interviewer.Activities
{
    public abstract class ProgressInterviewActivity<T> : BaseActivity<T> where T : ProgressViewModel
    {
        public abstract bool IsSupportMenu { get; }

        protected override int ViewResourceId => Resource.Layout.loading;

        protected override void OnCreate(Bundle savedInstanceState)
        {
            base.OnCreate(savedInstanceState);
            var toolbar = this.FindViewById<Toolbar>(Resource.Id.toolbar);
            this.SetSupportActionBar(toolbar);
        }

        public override void OnBackPressed()
        {
            if (IsSupportMenu)
            {
                this.ViewModel.NavigateToDashboardCommand.Execute();
                this.CancelLoadingAndFinishActivity();
            }
        }

        public override bool OnOptionsItemSelected(IMenuItem item)
        {
            if (!IsSupportMenu)
                return false;

            switch (item.ItemId)
            {
                case Resource.Id.menu_dashboard:
                    this.ViewModel.NavigateToDashboardCommand.Execute();
                    this.CancelLoadingAndFinishActivity();
                    break;
                case Resource.Id.menu_signout:
                    this.ViewModel.SignOutCommand.Execute();
                    this.CancelLoadingAndFinishActivity();
                    break;
                case Resource.Id.menu_settings:
                    Intent intent = new Intent(this, typeof(PrefsActivity));
                    this.StartActivity(intent);
                    this.CancelLoadingAndFinishActivity();
                    break;
                case Android.Resource.Id.Home:
                    this.CancelLoadingAndFinishActivity();
                    return true;
            }
            return base.OnOptionsItemSelected(item);
        }

        public override bool OnCreateOptionsMenu(IMenu menu)
        {
            if (IsSupportMenu)
            {
                this.MenuInflater.Inflate(Resource.Menu.diagnostics, menu);

                menu.LocalizeMenuItem(Resource.Id.menu_dashboard, EnumeratorUIResources.MenuItem_Title_Dashboard);
                menu.LocalizeMenuItem(Resource.Id.menu_settings, EnumeratorUIResources.MenuItem_Title_Settings);
                menu.LocalizeMenuItem(Resource.Id.menu_maps, EnumeratorUIResources.MenuItem_Title_Maps);
                menu.LocalizeMenuItem(Resource.Id.menu_signout, EnumeratorUIResources.MenuItem_Title_SignOut);

                this.HideMenuItem(menu, Resource.Id.menu_login);
            }

            return base.OnCreateOptionsMenu(menu);
        }

        private void HideMenuItem(IMenu menu, int menuItemId)
        {
            var menuItem = menu.FindItem(menuItemId);
            menuItem?.SetVisible(false);
        }

        private void CancelLoadingAndFinishActivity()
        {
            this.ViewModel.CancelLoadingCommand.Execute();
            this.Finish();
        }

        protected override void OnDestroy()
        {
            this.SetSupportActionBar(null);
            base.OnDestroy();
        }
    }
}
