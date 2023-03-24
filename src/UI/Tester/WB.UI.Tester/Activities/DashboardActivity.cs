using Android.App;
using Android.Content;
using Android.OS;
using Android.Views;
using AndroidX.AppCompat.Widget;
using AndroidX.RecyclerView.Widget;
using MvvmCross.DroidX.RecyclerView;
using WB.Core.BoundedContexts.Tester.Properties;
using WB.Core.BoundedContexts.Tester.ViewModels;
using WB.UI.Shared.Enumerator.Activities;
using WB.UI.Shared.Enumerator.Activities.Callbacks;
using Toolbar=AndroidX.AppCompat.Widget.Toolbar;

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

        protected override bool BackButtonCustomAction => true;
        protected override void BackButtonPressed()
        {
        }

        public override bool OnCreateOptionsMenu(IMenu menu)
        {
            this.MenuInflater.Inflate(Resource.Menu.dashboard, menu);

            menu.LocalizeMenuItem(Resource.Id.dashboard_anonymous_questionnaire, TesterUIResources.MenuItem_Title_AnonymousQuestionnaires);
            menu.LocalizeMenuItem(Resource.Id.dashboard_settings, TesterUIResources.MenuItem_Title_Settings);
            menu.LocalizeMenuItem(Resource.Id.dashboard_signout, TesterUIResources.MenuItem_Title_SignOut);

            return base.OnCreateOptionsMenu(menu);
        }

        public override bool OnOptionsItemSelected(IMenuItem item)
        {
            switch (item.ItemId)
            {
                case Resource.Id.dashboard_anonymous_questionnaire:
                    Intent intentAnonymousQuestionnaire = new Intent(this, typeof(AnonymousQuestionnairesActivity));
                    this.StartActivity(intentAnonymousQuestionnaire);
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

        protected override void OnDestroy()
        {
            ViewModel.CancelLoadServerQuestionnaires();
            base.OnDestroy();
        }
    }
}
