using Android.App;
using Android.OS;
using Android.Support.V7.App;
using Android.Support.V7.Widget;
using Android.Views;
using MvvmCross.Binding.Droid.BindingContext;
using MvvmCross.Core.Platform;
using MvvmCross.Droid.Support.V7.Fragging.Fragments;
using MvvmCross.Platform;
using MvvmCross.Droid.Support.V7.RecyclerView;
using MvvmCross.Plugins.Messenger;
using WB.Core.SharedKernels.Enumerator.ViewModels.InterviewDetails;
using WB.UI.Shared.Enumerator.CustomControls;
using Toolbar = Android.Support.V7.Widget.Toolbar;
using DrawerLayout = Android.Support.V4.Widget.DrawerLayout;

namespace WB.UI.Shared.Enumerator.Activities
{
    public abstract class EnumeratorInterviewActivity<TViewModel> : BaseActivity<TViewModel> where TViewModel : EnumeratorInterviewViewModel
    {
        private ActionBarDrawerToggle drawerToggle;
        private DrawerLayout drawerLayout;
        private MvxSubscriptionToken sectionChangeSubscriptionToken;
        private MvxSubscriptionToken interviewCompleteActivityToken;

        private Toolbar toolbar;

        protected override int ViewResourceId
        {
            get { return Resource.Layout.interview; }
        }

        protected override void OnCreate(Bundle bundle)
        {
            base.OnCreate(bundle);

            this.toolbar = this.FindViewById<Toolbar>(Resource.Id.toolbar);
            this.drawerLayout = this.FindViewById<DrawerLayout>(Resource.Id.drawer_layout);

            this.SetSupportActionBar(this.toolbar);
            this.SupportActionBar.SetDisplayHomeAsUpEnabled(true);
            this.SupportActionBar.SetHomeButtonEnabled(true);

            this.drawerToggle = new ActionBarDrawerToggle(this, this.drawerLayout, this.toolbar, 0, 0);
            this.drawerLayout.SetDrawerListener(this.drawerToggle);
            this.drawerLayout.DrawerOpened += (sender, args) =>
            {
                this.RemoveFocusFromEditText();
                this.HideKeyboard(drawerLayout.WindowToken);
                var viewModel = this.ViewModel;
                viewModel.Sections.UpdateStatuses.Execute(null); // for some reason custom binding on drawerlayout is not working. 
            };


            MvxFragment interviewEntitiesListFragment = new InterviewEntitiesListFragment();
            var frameViewModel = this.ViewModel.CurrentStage;
            interviewEntitiesListFragment.ViewModel = (ActiveStageViewModel)frameViewModel;

            var trans = SupportFragmentManager.BeginTransaction();
            trans.Replace(Resource.Id.interviewCurrentStepFrame, interviewEntitiesListFragment);
            trans.Commit();
        }

        protected override void OnStart()
        {
            var messenger = Mvx.Resolve<IMvxMessenger>();
            this.sectionChangeSubscriptionToken = messenger.Subscribe<SectionChangeMessage>(this.OnSectionChange);
            this.interviewCompleteActivityToken = messenger.Subscribe<InterviewCompletedMessage>(this.OnInterviewCompleteActivity);
            base.OnStart();
        }

        private void OnInterviewCompleteActivity(InterviewCompletedMessage obj)
        {
            this.Finish();
        }

        protected override void OnDestroy()
        {
            base.OnDestroy();

            if (IsFinishing)
            {
                this.Dispose();

                ViewModel.Dispose();
            }
        }

        private void OnSectionChange(SectionChangeMessage msg)
        {
            Application.SynchronizationContext.Post(_ => { this.drawerLayout.CloseDrawers();}, null);
        }

        protected override void OnStop()
        {
            var messenger = Mvx.Resolve<IMvxMessenger>();
            messenger.Unsubscribe<SectionChangeMessage>(this.sectionChangeSubscriptionToken);
            messenger.Unsubscribe<InterviewCompletedMessage>(this.interviewCompleteActivityToken);
            base.OnStop();
        }

        protected override void OnPostCreate(Bundle savedInstanceState)
        {
            base.OnPostCreate(savedInstanceState);
            this.drawerToggle.SyncState();
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