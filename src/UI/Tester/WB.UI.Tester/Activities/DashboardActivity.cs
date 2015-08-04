using Android.App;
using Android.Content;
using Android.OS;
using Android.Views;
using Android.Widget;
using Cirrious.MvvmCross.Binding.Droid.Views;
using WB.Core.BoundedContexts.Tester.ViewModels;
using Toolbar = Android.Support.V7.Widget.Toolbar;

namespace WB.UI.Tester.Activities
{
    [Activity(Label = "", Theme = "@style/GrayAppTheme", WindowSoftInputMode = SoftInput.StateHidden, ConfigurationChanges = Android.Content.PM.ConfigChanges.Orientation | Android.Content.PM.ConfigChanges.ScreenSize)]
    public class DashboardActivity : BaseActivity<DashboardViewModel>
    {
        protected override int ViewResourceId
        {
            get { return Resource.Layout.dashboard; }
        }


        protected override void OnCreate(Bundle bundle)
        {
            base.OnCreate(bundle);

            this.SetSupportActionBar(this.FindViewById<Toolbar>(Resource.Id.toolbar));

            var questionnairesList = this.FindViewById<MvxListView>(Resource.Id.questionnairesList);
            questionnairesList.EmptyView = this.FindViewById<LinearLayout>(Resource.Id.emptyView);
        }

        public override bool OnCreateOptionsMenu(IMenu menu)
        {
            this.MenuInflater.Inflate(Resource.Menu.dashboard, menu);
            return base.OnCreateOptionsMenu(menu);
        }
        public override bool OnOptionsItemSelected(IMenuItem item)
        {
            switch (item.ItemId)
            {
                case Resource.Id.dashboard_refresh:
                    this.ViewModel.RefreshQuestionnairesCommand.Execute();
                    break;
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
    }
}