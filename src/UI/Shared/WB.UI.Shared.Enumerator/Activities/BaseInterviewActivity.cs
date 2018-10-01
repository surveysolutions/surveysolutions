using System;
using Android.Content.Res;
using Android.OS;
using Android.Support.V4.Widget;
using Android.Support.V7.App;
using Android.Views;
using MvvmCross;
using MvvmCross.Base;
using MvvmCross.Plugin.Messenger;
using WB.Core.GenericSubdomains.Portable.Services;
using WB.Core.GenericSubdomains.Portable.Tasks;
using WB.Core.SharedKernels.DataCollection;
using WB.Core.SharedKernels.Enumerator.Services;
using WB.Core.SharedKernels.Enumerator.ViewModels;
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
            this.drawerLayout = this.FindViewById<DrawerLayout>(Resource.Id.rootLayout);
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
            try
            {
                this.Finish();
            }
            catch (ObjectDisposedException e)
            {
                Mvx.Resolve<ILoggerProvider>().GetForType(this.GetType()).Warn("Disposing already disposed activity", e);
            }
        }

        private async void OnSectionChange(SectionChangeMessage msg) =>
            await Mvx.Resolve<IMvxMainThreadAsyncDispatcher>().ExecuteOnMainThreadAsync(() =>
            {
                try
                {
                    this.drawerLayout.CloseDrawers();
                }
                catch(ArgumentException)
                {
                    //ignore System.ArgumentExceptionHandle must be valid. Parameter name: instance
                }
            });

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

        public override void OnLowMemory()
        {
            base.OnLowMemory();
            GC.Collect(GC.MaxGeneration);
        }

        protected override void OnStop()
        {
            base.OnStop();

            sectionChangeSubscriptionToken.Dispose();
            interviewCompleteActivityToken.Dispose();
            Mvx.Resolve<IAudioDialog>()?.StopRecordingAndSaveResult();
        }

        protected void Navigate(string navigateTo)
        {
            var parts = navigateTo.Split('|');
            this.ViewModel.navigationState.NavigateTo(new NavigationIdentity
            {
                TargetScreen = ScreenType.Group,
                TargetGroup = Identity.Parse(parts[0]),
                AnchoredElementIdentity = parts.Length > 1 ? Identity.Parse(parts[1]) : null
            }).WaitAndUnwrapException();
        }
    }
}
