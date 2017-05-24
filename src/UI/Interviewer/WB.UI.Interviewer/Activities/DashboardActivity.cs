using System.Collections.Generic;
using System.Linq;
using Android.App;
using Android.Content;
using Android.OS;
using Android.Support.Design.Widget;
using Android.Support.V4.View;
using Android.Support.V7.Widget;
using Android.Views;
using WB.Core.BoundedContexts.Interviewer.Properties;
using WB.Core.BoundedContexts.Interviewer.Services;
using WB.Core.BoundedContexts.Interviewer.Views.Dashboard;
using WB.UI.Interviewer.Services;
using WB.UI.Shared.Enumerator.Activities;

namespace WB.UI.Interviewer.Activities
{
    [Activity(Label = "", 
        Theme = "@style/GrayAppTheme", 
        WindowSoftInputMode = SoftInput.StateHidden,
        ConfigurationChanges = Android.Content.PM.ConfigChanges.Orientation | Android.Content.PM.ConfigChanges.ScreenSize)]
    public class DashboardActivity : BaseActivity<DashboardViewModel>, ISyncBgService
    {
        protected override int ViewResourceId => Resource.Layout.dashboard;

        public SyncServiceBinder Binder { get; set; }
        
        protected override void OnCreate(Bundle bundle)
        {
            base.OnCreate(bundle);

            this.SetSupportActionBar(this.FindViewById<Toolbar>(Resource.Id.toolbar));

            var viewPager = this.FindViewById<ViewPager>(Resource.Id.pager);
            var fragments = new List<MvxFragmentStatePagerAdapter.FragmentInfo>
            {
                new MvxFragmentStatePagerAdapter.FragmentInfo
                {
                    FragmentType = typeof(DashboardStartedInterviewsFragment),
                    ViewModel = ViewModel.StartedInterviews,
                    Title = ViewModel.StartedInterviews.Title
                },
                new MvxFragmentStatePagerAdapter.FragmentInfo
                {
                    FragmentType = typeof(DashboardNewInterviewsFragment),
                    ViewModel = ViewModel.NewInterviews,
                    Title = ViewModel.NewInterviews.Title
                },
                new MvxFragmentStatePagerAdapter.FragmentInfo
                {
                    FragmentType = typeof(DashboardRejectednterviewsFragment),
                    ViewModel = ViewModel.RejectedInterviews,
                    Title = ViewModel.RejectedInterviews.Title
                },
                new MvxFragmentStatePagerAdapter.FragmentInfo
                {
                    FragmentType = typeof(DashboardCompletednterviewsFragment),
                    ViewModel = ViewModel.CompletedInterviews,
                    Title = ViewModel.CompletedInterviews.Title
                },
                new MvxFragmentStatePagerAdapter.FragmentInfo
                {
                    FragmentType = typeof(DashboardQuestionnairesFragment),
                    ViewModel = ViewModel.Questionnaires,
                    Title = ViewModel.Questionnaires.Title
                },
            };

            var fragmentStatePagerAdapter = new MvxFragmentStatePagerAdapter(this, this.SupportFragmentManager, fragments);
            viewPager.Adapter = fragmentStatePagerAdapter;
            viewPager.PageSelected += (s, e) =>
            {
                ViewModel.TypeOfInterviews = ((InterviewTabPanel) fragments[e.Position].ViewModel).InterviewStatus;
            };

            var tabLayout = this.FindViewById<TabLayout>(Resource.Id.tabs);
            tabLayout.SetupWithViewPager(viewPager);

            for (int fragmentIndex = 0; fragmentIndex < fragments.Count; fragmentIndex++)
            {
                fragments[fragmentIndex].ViewModel.PropertyChanged += (s, e) =>
                {
                    if (e.PropertyName != nameof(InterviewTabPanel.Title)) return;

                    var tabIndex = fragments.FindIndex(fragmentInfo => fragmentInfo.ViewModel == s);
                    tabLayout.GetTabAt(tabIndex).SetText(((InterviewTabPanel)s).Title);
                };
            }
        }

        protected override void OnStart()
        {
            base.OnStart();
            BindService(new Intent(this, typeof(SyncBgService)), new SyncServiceConnection(this), Bind.AutoCreate);
        }

        protected override void OnViewModelSet()
        {
            base.OnViewModelSet();
            this.ViewModel.Synchronization.SyncBgService = this;
        }

        public override void OnBackPressed() {}

        public override bool OnCreateOptionsMenu(IMenu menu)
        {
            this.MenuInflater.Inflate(Resource.Menu.dashboard, menu);

            menu.LocalizeMenuItem(Resource.Id.menu_signout, InterviewerUIResources.MenuItem_Title_SignOut);
            menu.LocalizeMenuItem(Resource.Id.menu_settings, InterviewerUIResources.MenuItem_Title_Settings);
            menu.LocalizeMenuItem(Resource.Id.menu_diagnostics, InterviewerUIResources.MenuItem_Title_Diagnostics);
            return base.OnCreateOptionsMenu(menu);
        }

        public override bool OnOptionsItemSelected(IMenuItem item)
        {
            switch (item.ItemId)
            {
                case Resource.Id.menu_synchronization:
                    this.ViewModel.SynchronizationCommand.Execute();
                    break;
                case Resource.Id.menu_settings:
                    Intent intent = new Intent(this, typeof(PrefsActivity));
                    this.StartActivity(intent);
                    break;
                case Resource.Id.menu_diagnostics:
                    this.ViewModel.NavigateToDiagnosticsPageCommand.Execute();
                    break;
                case Resource.Id.menu_signout:
                    this.ViewModel.SignOutCommand.Execute();
                    break;
            }

            return base.OnOptionsItemSelected(item);
        }

        public void StartSync() => this.Binder.GetSyncService().StartSync();

        public SyncProgressDto CurrentProgress => this.Binder.GetSyncService().CurrentProgress;
    }
}