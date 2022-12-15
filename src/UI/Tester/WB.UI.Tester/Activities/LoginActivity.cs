using Android.Content;
using Android.Views;
using WB.Core.BoundedContexts.Tester.Properties;
using WB.Core.BoundedContexts.Tester.ViewModels;
using WB.UI.Shared.Enumerator.Activities;
using Toolbar=AndroidX.AppCompat.Widget.Toolbar;

namespace WB.UI.Tester.Activities
{
    [Activity(WindowSoftInputMode = SoftInput.StateHidden, 
        Theme = "@style/GrayAppTheme",
        Exported = false)]
    public class LoginActivity : BaseActivity<LoginViewModel>
    {
        protected override void OnCreate(Bundle bundle)
        {
            base.OnCreate(bundle);

            var toolbar = this.FindViewById<Toolbar>(Resource.Id.toolbar);
            toolbar.Title = "";
            this.SetSupportActionBar(toolbar);
        }

        public override void OnBackPressed()
        {
        }

        public override bool OnCreateOptionsMenu(IMenu menu)
        {
            this.MenuInflater.Inflate(Resource.Menu.login, menu);

            menu.LocalizeMenuItem(Resource.Id.login_anonymous_questionnaire, TesterUIResources.MenuItem_Title_AnonymousQuestionnaires);
            menu.LocalizeMenuItem(Resource.Id.login_settings, TesterUIResources.MenuItem_Title_Settings);

            return base.OnCreateOptionsMenu(menu);
        }

        public override bool OnOptionsItemSelected(IMenuItem item)
        {
            switch (item.ItemId)
            {
                case Resource.Id.login_anonymous_questionnaire:
                    this.StartActivity(new Intent(this, typeof(AnonymousQuestionnairesActivity)));
                    break;
                case Resource.Id.login_settings:
                    this.StartActivity(new Intent(this, typeof(PrefsActivity)));
                    break;
                default: break;
            }

            return base.OnOptionsItemSelected(item);
        }

        protected override int ViewResourceId
        {
            get { return Resource.Layout.login; }
        }
    }
}
