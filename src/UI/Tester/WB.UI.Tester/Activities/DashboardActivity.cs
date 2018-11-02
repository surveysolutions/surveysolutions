using System;
using Android.App;
using Android.Content;
using Android.OS;
using Android.Support.V7.Widget;
using Android.Views;
using MvvmCross.Droid.Support.V7.RecyclerView;
using WB.Core.BoundedContexts.Tester.Properties;
using WB.Core.BoundedContexts.Tester.ViewModels;
using WB.UI.Shared.Enumerator.Activities;

using Toolbar = Android.Support.V7.Widget.Toolbar;

namespace WB.UI.Tester.Activities
{
    [Activity(Label = "",
        Theme = "@style/GrayAppTheme", 
        WindowSoftInputMode = SoftInput.StateHidden, 
        ConfigurationChanges = Android.Content.PM.ConfigChanges.Orientation | Android.Content.PM.ConfigChanges.ScreenSize,
        Exported = false)]
    public class DashboardActivity : BaseActivity<DashboardViewModel>
    {
        protected override int ViewResourceId => Resource.Layout.dashboard;

        protected override void OnCreate(Bundle bundle)
        {
            base.OnCreate(bundle);

            this.SetSupportActionBar(this.FindViewById<Toolbar>(Resource.Id.toolbar));

            var recyclerView = this.FindViewById<MvxRecyclerView>(Resource.Id.questionnairesList);
            var layoutManager = new LinearLayoutManager(this);
            recyclerView.SetLayoutManager(layoutManager);
        }

        public override void OnBackPressed()
        {
        }

        public override bool OnCreateOptionsMenu(IMenu menu)
        {
            this.MenuInflater.Inflate(Resource.Menu.dashboard, menu);

            menu.LocalizeMenuItem(Resource.Id.dashboard_settings, TesterUIResources.MenuItem_Title_Settings);
            menu.LocalizeMenuItem(Resource.Id.dashboard_signout, TesterUIResources.MenuItem_Title_SignOut);

            return base.OnCreateOptionsMenu(menu);
        }

        public override bool OnOptionsItemSelected(IMenuItem item)
        {
            switch (item.ItemId)
            {
                case Resource.Id.dashboard_settings:
                    Intent intent = new Intent(this, typeof(PrefsActivity));
                    this.StartActivity(intent);
                    break;
                case Resource.Id.dashboard_signout:
                    this.ViewModel.SignOutCommand.Execute();
                    break;
            }
            return base.OnOptionsItemSelected(item);
        }

        protected override void OnDestroy()
        {
            ViewModel.CancelLoadServerQuestionnaires();
            base.OnDestroy();
        }
    }
}
