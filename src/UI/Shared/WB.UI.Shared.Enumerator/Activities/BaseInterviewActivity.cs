using Android.App;
using Android.OS;
using Android.Support.V4.Widget;
using Android.Support.V7.App;
using Android.Views;
using MvvmCross.Platform;
using MvvmCross.Plugins.Messenger;
using WB.Core.SharedKernels.Enumerator.ViewModels.InterviewDetails;

namespace WB.UI.Shared.Enumerator.Activities
{
    public abstract class BaseInterviewActivity<TViewModel> : SingleInterviewActivity<TViewModel>
        where TViewModel : BaseInterviewViewModel
    {
        private ActionBarDrawerToggle drawerToggle;
        private DrawerLayout drawerLayout;
        private MvxSubscriptionToken sectionChangeSubscriptionToken;
        private MvxSubscriptionToken interviewCompleteActivityToken;

        protected override int ViewResourceId => Resource.Layout.interview;

        protected override void OnCreate(Bundle bundle)
        {
            base.OnCreate(bundle);

            this.drawerLayout = this.FindViewById<DrawerLayout>(Resource.Id.drawer_layout);
            this.drawerToggle = new ActionBarDrawerToggle(this, this.drawerLayout, this.toolbar, 0, 0);
            this.drawerLayout.AddDrawerListener(this.drawerToggle);
            this.drawerLayout.DrawerOpened += (sender, args) =>
            {
                this.RemoveFocusFromEditText();
                this.HideKeyboard(drawerLayout.WindowToken);
            };
        }

        protected override void OnStart()
        {
            var messenger = Mvx.Resolve<IMvxMessenger>();

            this.sectionChangeSubscriptionToken = messenger.Subscribe<SectionChangeMessage>(this.OnSectionChange);
            this.interviewCompleteActivityToken = messenger.Subscribe<InterviewCompletedMessage>(this.OnInterviewCompleteActivity);
            base.OnStart();
        }

        public override void OnBackPressed()
        {
            this.ViewModel.NavigateToPreviousViewModel(() =>
            {
                this.ViewModel.NavigateBack();
                this.Finish();
            });
        }

        public override bool OnOptionsItemSelected(IMenuItem item)
            => this.drawerToggle.OnOptionsItemSelected(item)
            || base.OnOptionsItemSelected(item);

        private void OnInterviewCompleteActivity(InterviewCompletedMessage obj)
        {
            this.Finish();
        }

        protected override void Dispose(bool disposing)
        {
            base.Dispose(disposing);

            this.ViewModel?.Dispose();
        }

        protected override void OnDestroy()
        {
            base.OnDestroy();

            if (IsFinishing)
            {
                var messenger = Mvx.Resolve<IMvxMessenger>();
                messenger.Unsubscribe<SectionChangeMessage>(this.sectionChangeSubscriptionToken);
                messenger.Unsubscribe<InterviewCompletedMessage>(this.interviewCompleteActivityToken);

                this.ViewModel.Sections.Dispose();

                this.Dispose();
            }
        }

        private void OnSectionChange(SectionChangeMessage msg)
        {
            Application.SynchronizationContext.Post(_ => { this.drawerLayout.CloseDrawers(); }, null);
        }

        protected override void OnPostCreate(Bundle savedInstanceState)
        {
            base.OnPostCreate(savedInstanceState);
            this.drawerToggle.SyncState();
        }
    }
}