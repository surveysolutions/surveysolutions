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

namespace WB.UI.Tester.Activities
{
    [Activity(Label = "",
        Theme = "@style/GrayAppTheme",
        WindowSoftInputMode = SoftInput.StateHidden,
        ConfigurationChanges =
            Android.Content.PM.ConfigChanges.Orientation | Android.Content.PM.ConfigChanges.ScreenSize,
        Exported = false)]
    public class AnonymousQuestionnairesActivity : BaseActivity<AnonymousQuestionnairesViewModel>
    {
        protected override int ViewResourceId => Resource.Layout.anonymousquestionnaires;

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
            this.Finish();
        }

        public override bool OnCreateOptionsMenu(IMenu menu)
        {
            this.MenuInflater.Inflate(Resource.Menu.anonymousquestionnaires, menu);

            menu.LocalizeMenuItem(Resource.Id.anonymousquestionnaires_settings,
                TesterUIResources.MenuItem_Title_Settings);
            menu.LocalizeMenuItem(Resource.Id.anonymousquestionnaires_login, TesterUIResources.MenuItem_Title_Login);
            menu.LocalizeMenuItem(Resource.Id.anonymousquestionnaires_signout,
                TesterUIResources.MenuItem_Title_SignOut);

            var isAuthenticated = ViewModel.IsAuthenticated;
            menu.VisibleMenuItem(Resource.Id.anonymousquestionnaires_login, !isAuthenticated);
            menu.VisibleMenuItem(Resource.Id.anonymousquestionnaires_signout, isAuthenticated);

            return base.OnCreateOptionsMenu(menu);
        }

        public override bool OnOptionsItemSelected(IMenuItem item)
        {
            switch (item.ItemId)
            {
                case Resource.Id.anonymousquestionnaires_login:
                    this.StartActivity(new Intent(this, typeof(LoginActivity)));
                    break;
                case Resource.Id.anonymousquestionnaires_signout:
                    this.ViewModel.SignOutCommand.Execute();
                    break;
                case Resource.Id.anonymousquestionnaires_settings:
                    this.StartActivity(new Intent(this, typeof(PrefsActivity)));
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