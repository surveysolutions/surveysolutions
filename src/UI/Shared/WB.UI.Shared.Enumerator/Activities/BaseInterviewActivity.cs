using System.ComponentModel;
using Android.Content.Res;
using Android.Views;
using AndroidX.AppCompat.App;
using AndroidX.DrawerLayout.Widget;
using AndroidX.RecyclerView.Widget;
using MvvmCross;
using MvvmCross.Plugin.Messenger;
using MvvmCross.WeakSubscription;
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
        
        private IDisposable onNavigationSubscription;
        private IDisposable onDrawerOpenedSubscription;

        protected override int ViewResourceId => Resource.Layout.interview;

        protected override void OnCreate(Bundle bundle)
        {
            base.OnCreate(bundle);
            this.drawerLayout = this.FindViewById<DrawerLayout>(Resource.Id.rootLayout);
            this.drawerToggle = new ActionBarDrawerToggle(this, this.drawerLayout, base.Toolbar, 0, 0);
            this.drawerLayout.AddDrawerListener(this.drawerToggle);
            
            onDrawerOpenedSubscription = this.drawerLayout.WeakSubscribe<DrawerLayout, DrawerLayout.DrawerOpenedEventArgs>(
                nameof(this.drawerLayout.DrawerOpened),
                OnDrawerLayoutOnDrawerOpened)  ;
        }
        
        protected override bool BackButtonCustomAction => true;
        protected override void BackButtonPressed()
        {
            this.ViewModel.NavigateToPreviousViewModel(() =>
            {
                this.ViewModel.NavigateBack();
                ReleaseActivity();
            });
        }

        private void OnDrawerLayoutOnDrawerOpened(object sender, DrawerLayout.DrawerOpenedEventArgs args)
        {
            this.RemoveFocusFromEditText();
            this.HideKeyboard(drawerLayout.WindowToken);
        }

        protected override void OnStart()
        {
            var messenger = Mvx.IoCProvider.GetSingleton<IMvxMessenger>();

            this.sectionChangeSubscriptionToken = messenger.Subscribe<SectionChangeMessage>(this.OnSectionChange);
            this.interviewCompleteActivityToken = messenger.Subscribe<InterviewCompletedMessage>(this.OnInterviewCompleteActivity);
            base.OnStart();
        }

        public override bool OnOptionsItemSelected(IMenuItem item)
            => this.drawerToggle.OnOptionsItemSelected(item)
            || base.OnOptionsItemSelected(item);

        protected void ReleaseActivity()
        {
            try
            {
                onNavigationSubscription?.Dispose();
                onNavigationSubscription = null;
                
                onDrawerOpenedSubscription?.Dispose();
                onDrawerOpenedSubscription = null;
                
                this.ViewModel.Dispose();
                this.Finish();
            }
            catch (ObjectDisposedException e)
            {
                Mvx.IoCProvider.Resolve<ILoggerProvider>().GetForType(this.GetType()).Warn("Disposing already disposed activity", e);
            }
        }

        private void OnInterviewCompleteActivity(InterviewCompletedMessage obj)
        {
            ReleaseActivity();
        }

        private void OnSectionChange(SectionChangeMessage msg) =>
            RunOnUiThread(() =>
            {
                try
                {
                    this.drawerLayout.CloseDrawers();
                }
                catch (ArgumentException)
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
            Mvx.IoCProvider.Resolve<IAudioDialog>()?.StopRecordingAndSaveResult();
        }

        protected override void OnViewModelSet()
        {
            base.OnViewModelSet();
            onNavigationSubscription = ViewModel.WeakSubscribe<PropertyChangedEventArgs>(nameof(ViewModel.CurrentStage), OnNavigation);
        }

        private void OnNavigation(object sender, PropertyChangedEventArgs e)
        {
            if (ViewModel.CurrentStage.Stage is EnumerationStageViewModel)
            {
                var list = FindViewById<RecyclerView>(Resource.Id.interviewEntitiesList);
                list?.SetItemAnimator(null);
            }
        }

        protected void Navigate(string navigateTo)
        {
            var parts = navigateTo.Split('|');
            this.ViewModel.NavigationState.NavigateTo(new NavigationIdentity
            {
                TargetScreen = ScreenType.Group,
                TargetGroup = Identity.Parse(parts[0]),
                AnchoredElementIdentity = parts.Length > 1 ? Identity.Parse(parts[1]) : null
            }).WaitAndUnwrapException();
        }
    }
}
