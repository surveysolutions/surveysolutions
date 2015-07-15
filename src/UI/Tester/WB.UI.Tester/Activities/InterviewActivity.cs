using Android.App;
using Android.Content;
using Android.OS;
using Android.Support.V4.Widget;
using Android.Support.V7.App;
using Android.Support.V7.Widget;
using Android.Views;
using Cirrious.CrossCore;
using Cirrious.MvvmCross.Binding.Droid.BindingContext;
using Cirrious.MvvmCross.Plugins.Messenger;

using Java.Lang;

using WB.Core.BoundedContexts.Tester.ViewModels;
using WB.UI.Tester.CustomControls;

using Toolbar = Android.Support.V7.Widget.Toolbar;

namespace WB.UI.Tester.Activities
{
    [Activity(Label = "", Theme = "@style/BlueAppTheme", HardwareAccelerated = true,
        WindowSoftInputMode = SoftInput.StateAlwaysHidden | SoftInput.AdjustPan,
        ConfigurationChanges = Android.Content.PM.ConfigChanges.Orientation | Android.Content.PM.ConfigChanges.ScreenSize)]
    public class InterviewActivity : BaseActivity<InterviewViewModel>
    {
        private ActionBarDrawerToggle drawerToggle;
        private DrawerLayout drawerLayout;
        private MvxSubscriptionToken sectionChangeSubscriptionToken;
        private MvxSubscriptionToken scrollToAnchorSubscriptionToken;
        private MvxSubscriptionToken updateEntityStateSubscriptionToken;

        private Toolbar toolbar;

        private MvxRecyclerView recyclerView;

        private LinearLayoutManager layoutManager;

        private InterviewEntityAdapter adapter;

        protected override int ViewResourceId
        {
            get { return Resource.Layout.interview; }
        }

        protected override void OnCreate(Bundle bundle)
        {
            base.OnCreate(bundle);

            this.toolbar = this.FindViewById<Toolbar>(Resource.Id.toolbar);
            drawerLayout = this.FindViewById<DrawerLayout>(Resource.Id.drawer_layout);

            this.recyclerView = this.FindViewById<MvxRecyclerView>(Resource.Id.questionnaireEntitiesList);

            this.SetSupportActionBar(this.toolbar);
            this.SupportActionBar.SetDisplayHomeAsUpEnabled(true);
            this.SupportActionBar.SetHomeButtonEnabled(true);

            this.drawerToggle = new ActionBarDrawerToggle(this, drawerLayout, this.toolbar, 0, 0);
            drawerLayout.SetDrawerListener(this.drawerToggle);
            drawerLayout.DrawerOpened += (sender, args) =>
            {
                this.RemoveFocusFromEditText();
                this.HideKeyboard(drawerLayout.WindowToken);
            };

            this.layoutManager = new LinearLayoutManager(this);
            this.recyclerView.SetLayoutManager(this.layoutManager);
            this.recyclerView.HasFixedSize = true;

            this.adapter = new InterviewEntityAdapter(this, (IMvxAndroidBindingContext)this.BindingContext);
            
            this.recyclerView.Adapter = this.adapter;
        }

        protected override void OnStart()
        {
            var messenger = Mvx.Resolve<IMvxMessenger>();
            sectionChangeSubscriptionToken = messenger.Subscribe<SectionChangeMessage>(this.OnSectionChange);
            scrollToAnchorSubscriptionToken = messenger.Subscribe<ScrollToAnchorMessage>(this.OnScrollToAnchorMessage);
            this.updateEntityStateSubscriptionToken = messenger.Subscribe<UpdateInterviewEntityStateMessage>(this.OnUpdateQuestionState);
            base.OnStart();
        }

        private void OnSectionChange(SectionChangeMessage msg)
        {
            Application.SynchronizationContext.Post(_ =>
            {
                drawerLayout.CloseDrawers();
            },
            null);
        }

        private void OnUpdateQuestionState(UpdateInterviewEntityStateMessage msg)
        {
            Application.SynchronizationContext.Post(_ =>
            {
                try
                {
                    adapter.NotifyItemChanged(msg.ElementPosition);
                }
                catch (IllegalStateException){}
            },
            null);
        }

        private void OnScrollToAnchorMessage(ScrollToAnchorMessage msg)
        {
            if (this.layoutManager != null)
            {
                Application.SynchronizationContext.Post(_ =>
                    {
                        this.layoutManager.ScrollToPositionWithOffset(msg.AnchorElementIndex, 0);
                    },
                    null);
            }
        }

        protected override void OnStop()
        {
            var messenger = Mvx.Resolve<IMvxMessenger>();
            messenger.Unsubscribe<SectionChangeMessage>(sectionChangeSubscriptionToken);
            messenger.Unsubscribe<ScrollToAnchorMessage>(scrollToAnchorSubscriptionToken);
            messenger.Unsubscribe<UpdateInterviewEntityStateMessage>(this.updateEntityStateSubscriptionToken);
            base.OnStop();
        }

        protected override void OnPostCreate(Bundle savedInstanceState)
        {
            base.OnPostCreate(savedInstanceState);
            this.drawerToggle.SyncState();
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