using Android.App;
using Android.Content;
using Android.Content.Res;
using Android.OS;
using Android.Support.V4.Widget;
using Android.Support.V7.App;
using Android.Support.V7.Widget;
using Android.Views;
using Cirrious.MvvmCross.Binding.Droid.BindingContext;
using WB.Core.BoundedContexts.QuestionnaireTester.ViewModels;
using WB.UI.QuestionnaireTester.CustomControls;

namespace WB.UI.QuestionnaireTester.Activities
{
    [Activity(Label = "", Theme = "@style/BlueAppTheme", HardwareAccelerated = true, WindowSoftInputMode = SoftInput.StateHidden)]
    public class InterviewActivity : BaseActivity<InterviewViewModel>
    {
        private ActionBarDrawerToggle drawerToggle;

        protected override int ViewResourceId
        {
            get { return Resource.Layout.interview; }
        }

        protected override void OnCreate(Bundle bundle)
        {
            base.OnCreate(bundle);

            var toolbar = this.FindViewById<Toolbar>(Resource.Id.toolbar);
            var drawerLayout = this.FindViewById<DrawerLayout>(Resource.Id.drawer_layout);
            var listOfInterviewQuestionsAndGroups = this.FindViewById<MvxRecyclerView>(Resource.Id.questionnaireEntitiesList);

            this.SetSupportActionBar(toolbar);
            this.SupportActionBar.SetDisplayHomeAsUpEnabled(true);
            this.SupportActionBar.SetHomeButtonEnabled(true);

            this.drawerToggle = new ActionBarDrawerToggle(this, drawerLayout, toolbar, 0, 0);

            drawerLayout.SetDrawerListener(this.drawerToggle);

            var layoutManager = new LinearLayoutManager(this);;
            listOfInterviewQuestionsAndGroups.SetLayoutManager(layoutManager);
            listOfInterviewQuestionsAndGroups.HasFixedSize = true;
            listOfInterviewQuestionsAndGroups.Adapter = new InterviewEntityAdapter(this, (IMvxAndroidBindingContext)this.BindingContext);
        }

        protected override void OnPostCreate(Bundle savedInstanceState)
        {
            base.OnPostCreate(savedInstanceState);
            this.drawerToggle.SyncState();
        }

        public override void OnConfigurationChanged(Configuration newConfig)
        {
            base.OnConfigurationChanged(newConfig);
            this.drawerToggle.OnConfigurationChanged(newConfig);
        }

        public override bool OnCreateOptionsMenu(IMenu menu)
        {
            this.MenuInflater.Inflate(Resource.Menu.interview, menu);
            return base.OnCreateOptionsMenu(menu);
        }

        public override bool OnOptionsItemSelected(IMenuItem item)
        {
            if (this.drawerToggle.OnOptionsItemSelected(item))
            {
                return true;
            }

            switch (item.ItemId)
            {
                case Resource.Id.interview_dashboard:
                    this.ViewModel.NavigateToDashboardCommand.Execute();
                    break;
                case Resource.Id.interview_help:
                    this.ViewModel.NavigateToHelpCommand.Execute();
                    break;
                case Resource.Id.interview_language:
                    this.ViewModel.ChangeLanguageCommand.Execute();
                    break;
                case Resource.Id.interview_settings:
                    Intent intent = new Intent(this, typeof(PrefsActivity));
                    this.StartActivity(intent);
                    break;
                case Resource.Id.interview_signout:
                    this.ViewModel.SignOutCommand.Execute();
                    break;

            }
            return base.OnOptionsItemSelected(item);
        }
    }
}
