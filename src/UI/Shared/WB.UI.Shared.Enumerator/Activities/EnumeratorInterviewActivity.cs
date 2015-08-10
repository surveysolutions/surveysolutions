using Android.App;
using Android.OS;
using Android.Support.V4.Widget;
using Android.Support.V7.App;
using Android.Support.V7.Widget;
using Android.Views;
using Cirrious.CrossCore;
using Cirrious.MvvmCross.Binding.Droid.BindingContext;
using Cirrious.MvvmCross.Plugins.Messenger;
using WB.Core.SharedKernels.Enumerator.ViewModels;
using WB.Core.SharedKernels.Enumerator.ViewModels.InterviewDetails;
using WB.UI.Shared.Enumerator.CustomControls;
using Toolbar = Android.Support.V7.Widget.Toolbar;

namespace WB.UI.Shared.Enumerator.Activities
{
    public abstract class EnumeratorInterviewActivity<TViewModel> : BaseActivity<TViewModel> where TViewModel : BaseViewModel
    {
        private ActionBarDrawerToggle drawerToggle;
        private DrawerLayout drawerLayout;
        private MvxSubscriptionToken sectionChangeSubscriptionToken;
        private MvxSubscriptionToken scrollToAnchorSubscriptionToken;

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
            this.drawerLayout = this.FindViewById<DrawerLayout>(Resource.Id.drawer_layout);

            this.recyclerView = this.FindViewById<MvxRecyclerView>(Resource.Id.interviewEntitiesList);

            this.SetSupportActionBar(this.toolbar);
            this.SupportActionBar.SetDisplayHomeAsUpEnabled(true);
            this.SupportActionBar.SetHomeButtonEnabled(true);

            this.drawerToggle = new ActionBarDrawerToggle(this, this.drawerLayout, this.toolbar, 0, 0);
            this.drawerLayout.SetDrawerListener(this.drawerToggle);
            this.drawerLayout.DrawerOpened += (sender, args) =>
            {
                this.RemoveFocusFromEditText();
                this.HideKeyboard(this.drawerLayout.WindowToken);
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
            this.sectionChangeSubscriptionToken = messenger.Subscribe<SectionChangeMessage>(this.OnSectionChange);
            this.scrollToAnchorSubscriptionToken = messenger.Subscribe<ScrollToAnchorMessage>(this.OnScrollToAnchorMessage);
            base.OnStart();
        }

        protected override void OnDestroy()
        {
            base.OnDestroy();
            this.Dispose();
        }

        private void OnSectionChange(SectionChangeMessage msg)
        {
            Application.SynchronizationContext.Post(_ => { this.drawerLayout.CloseDrawers();}, null);
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
            messenger.Unsubscribe<SectionChangeMessage>(this.sectionChangeSubscriptionToken);
            messenger.Unsubscribe<ScrollToAnchorMessage>(this.scrollToAnchorSubscriptionToken);
            base.OnStop();
        }

        protected override void OnPostCreate(Bundle savedInstanceState)
        {
            base.OnPostCreate(savedInstanceState);
            this.drawerToggle.SyncState();
        }

        public override bool OnCreateOptionsMenu(IMenu menu)
        {
            this.MenuInflater.Inflate(this.MenuResourceId, menu);
            return base.OnCreateOptionsMenu(menu);
        }

        public override bool OnOptionsItemSelected(IMenuItem item)
        {
            if (this.drawerToggle.OnOptionsItemSelected(item))
            {
                return true;
            }

            this.OnMenuItemSelected(item.ItemId);

            return base.OnOptionsItemSelected(item);
        }

        protected abstract int MenuResourceId { get; }
        protected abstract void OnMenuItemSelected(int resourceId);
    }
}