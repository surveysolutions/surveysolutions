using Android.App;
using Android.Content;
using Android.Content.PM;
using Android.OS;
using Android.Support.V4.Widget;
using Android.Support.V7.App;
using Android.Views;
using MvvmCross.Platform;
using MvvmCross.Plugins.Messenger;
using Plugin.CurrentActivity;
using Plugin.Permissions;
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
        private MvxSubscriptionToken countOfInvalidEntitiesIncreasedToken;
        private Vibrator vibrator;

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
                var viewModel = this.ViewModel;
                viewModel.Sections.UpdateStatuses.Execute(null); // for some reason custom binding on drawerlayout is not working. 
            };

            CrossCurrentActivity.Current.Activity = this;
        }

        protected override void OnResume()
        {
            base.OnResume();
            CrossCurrentActivity.Current.Activity = this;
        }

        protected override void OnStart()
        {
            this.vibrator = (Vibrator)this.GetSystemService(Context.VibratorService);

            var messenger = Mvx.Resolve<IMvxMessenger>();

            this.sectionChangeSubscriptionToken = messenger.Subscribe<SectionChangeMessage>(this.OnSectionChange);
            this.interviewCompleteActivityToken = messenger.Subscribe<InterviewCompletedMessage>(this.OnInterviewCompleteActivity);
            this.countOfInvalidEntitiesIncreasedToken = messenger.Subscribe<CountOfInvalidEntitiesIncreasedMessage>(this.OnCountOfInvalidEntitiesIncreased);
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

        public override void OnRequestPermissionsResult(int requestCode, string[] permissions, Permission[] grantResults)
        {
            base.OnRequestPermissionsResult(requestCode, permissions, grantResults);
            PermissionsImplementation.Current.OnRequestPermissionsResult(requestCode, permissions, grantResults);
        }

        public override bool OnOptionsItemSelected(IMenuItem item)
            => this.drawerToggle.OnOptionsItemSelected(item)
            || base.OnOptionsItemSelected(item);

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

        private void OnCountOfInvalidEntitiesIncreased(CountOfInvalidEntitiesIncreasedMessage msg)
        {
            if (this.vibrator.HasVibrator)
                vibrator.Vibrate(100);
        }

        private void OnSectionChange(SectionChangeMessage msg)
        {
            Application.SynchronizationContext.Post(_ => { this.drawerLayout.CloseDrawers(); }, null);
        }

        protected override void OnStop()
        {
            var messenger = Mvx.Resolve<IMvxMessenger>();
            messenger.Unsubscribe<SectionChangeMessage>(this.sectionChangeSubscriptionToken);
            messenger.Unsubscribe<InterviewCompletedMessage>(this.interviewCompleteActivityToken);
            messenger.Unsubscribe<CountOfInvalidEntitiesIncreasedMessage>(this.countOfInvalidEntitiesIncreasedToken);
            base.OnStop();
        }

        protected override void OnPostCreate(Bundle savedInstanceState)
        {
            base.OnPostCreate(savedInstanceState);
            this.drawerToggle.SyncState();
        }
    }
}