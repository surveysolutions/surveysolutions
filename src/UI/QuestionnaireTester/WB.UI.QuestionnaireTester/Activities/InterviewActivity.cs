using Android.App;
using Android.Content;
using Android.Content.Res;
using Android.OS;
using Android.Support.V4.Widget;
using Android.Support.V7.App;
using Android.Support.V7.Widget;
using Android.Views;
using Cirrious.CrossCore;
using Cirrious.MvvmCross.Binding.Droid.BindingContext;
using Cirrious.MvvmCross.Plugins.Messenger;
using WB.Core.BoundedContexts.QuestionnaireTester.ViewModels;
using WB.UI.QuestionnaireTester.CustomControls;

namespace WB.UI.QuestionnaireTester.Activities
{
    [Activity(Label = "", Theme = "@style/BlueAppTheme", HardwareAccelerated = true, WindowSoftInputMode = SoftInput.StateAlwaysHidden, 
        ConfigurationChanges = Android.Content.PM.ConfigChanges.Orientation | Android.Content.PM.ConfigChanges.ScreenSize)]
    public class InterviewActivity : BaseActivity<InterviewViewModel>
    {
        private ActionBarDrawerToggle drawerToggle;
        private DrawerLayout drawerLayout;
        private MvxSubscriptionToken sectionChangeSubscriptionToken;
        private MvxSubscriptionToken scrollToAnchorSubscriptionToken;

        private LinearLayoutManager layoutManager;

        private Toolbar toolbar;

        private MvxRecyclerView listOfInterviewQuestionsAndGroups;

        protected override int ViewResourceId
        {
            get { return Resource.Layout.interview; }
        }

        protected override void OnCreate(Bundle bundle)
        {
            base.OnCreate(bundle);

            this.toolbar = this.FindViewById<Toolbar>(Resource.Id.toolbar);
            drawerLayout = this.FindViewById<DrawerLayout>(Resource.Id.drawer_layout);
            
            this.listOfInterviewQuestionsAndGroups = this.FindViewById<MvxRecyclerView>(Resource.Id.questionnaireEntitiesList);

            this.SetSupportActionBar(this.toolbar);
            this.SupportActionBar.SetDisplayHomeAsUpEnabled(true);
            this.SupportActionBar.SetHomeButtonEnabled(true);

            this.drawerToggle = new ActionBarDrawerToggle(this, drawerLayout, this.toolbar, 0, 0);

            drawerLayout.SetDrawerListener(this.drawerToggle);

            this.layoutManager = new LinearLayoutManager(this);
            this.listOfInterviewQuestionsAndGroups.SetLayoutManager(this.layoutManager);
            this.listOfInterviewQuestionsAndGroups.HasFixedSize = true;
            this.listOfInterviewQuestionsAndGroups.Adapter = new InterviewEntityAdapter(this, (IMvxAndroidBindingContext)this.BindingContext);

             var messenger = Mvx.Resolve<IMvxMessenger>();
             sectionChangeSubscriptionToken = messenger.Subscribe<SectionChangeMessage>(this.OnSectionChange);
             scrollToAnchorSubscriptionToken = messenger.Subscribe<ScrollToAnchorMessage>(this.OnScrollToAnchorMessage);
        }

        private void OnSectionChange(SectionChangeMessage msg)
        {
            drawerLayout.CloseDrawers();
        }

        private void OnScrollToAnchorMessage(ScrollToAnchorMessage msg)
        {
            var offset = 0;
            if (msg.OffsetInsideOfAnchoredItemInPercentage > 0)
            {
                var anchoredView = layoutManager.FindViewByPosition(msg.AnchorElementIndex);
                
                if (anchoredView!=null)
                {
                    offset = anchoredView.Height * msg.OffsetInsideOfAnchoredItemInPercentage / 100;
                }
            }
            this.layoutManager.ScrollToPositionWithOffset(msg.AnchorElementIndex, offset);
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
