using System;
using System.ComponentModel;
using Android.App;
using Android.Content;
using Android.OS;
using Android.Support.Design.Widget;
using Android.Support.V4.View;
using Android.Support.V7.Widget;
using Android.Views;
using MvvmCross.Droid.Support.V4;
using WB.Core.BoundedContexts.Interviewer.Properties;
using WB.Core.BoundedContexts.Interviewer.Services;
using WB.Core.BoundedContexts.Interviewer.Views.Dashboard;
using WB.Core.BoundedContexts.Interviewer.Views.Dashboard.DashboardItems;
using WB.UI.Interviewer.Services;
using WB.UI.Shared.Enumerator.Activities;
using MvxFragmentStatePagerAdapter = WB.UI.Interviewer.CustomControls.MvxFragmentStatePagerAdapter;

namespace WB.UI.Interviewer.Activities.Dashboard
{
    [Activity(Label = "", 
        Theme = "@style/GrayAppTheme", 
        WindowSoftInputMode = SoftInput.StateHidden,
        HardwareAccelerated = true,
        ConfigurationChanges = Android.Content.PM.ConfigChanges.Orientation | Android.Content.PM.ConfigChanges.ScreenSize)]
    public class DashboardActivity : BaseActivity<DashboardViewModel>, ISyncBgService
    {
        protected override int ViewResourceId => Resource.Layout.dashboard;

        public SyncServiceBinder Binder { get; set; }

        private MvxFragmentStatePagerAdapter fragmentStatePagerAdapter;
        private ViewPager viewPager;

        protected override void OnSaveInstanceState(Bundle outState)
        {
            this.RemoveFragments();

            base.OnSaveInstanceState(outState);
        }

        protected override void OnCreate(Bundle bundle)
        {
            base.OnCreate(bundle);

            this.SetSupportActionBar(this.FindViewById<Toolbar>(Resource.Id.toolbar));
        }

        private void RemoveFragments()
        {
            this.fragmentStatePagerAdapter.RemoveAllFragments();
            this.viewPager.PageSelected -= this.ViewPager_PageSelected;

            this.ViewModel.StartedInterviews.PropertyChanged -= this.StartedInterviews_PropertyChanged;
            this.ViewModel.RejectedInterviews.PropertyChanged -= this.RejectedInterviews_PropertyChanged;
            this.ViewModel.CompletedInterviews.PropertyChanged -= this.CompletedInterviews_PropertyChanged;
        }

        private void CreateFragments()
        {
            this.viewPager = this.FindViewById<ViewPager>(Resource.Id.pager);

            this.fragmentStatePagerAdapter = new MvxFragmentStatePagerAdapter(this, this.viewPager, this.SupportFragmentManager);
            this.viewPager.Adapter = this.fragmentStatePagerAdapter;
            this.viewPager.PageSelected += this.ViewPager_PageSelected;

            this.ViewModel.StartedInterviews.PropertyChanged += this.StartedInterviews_PropertyChanged;
            this.ViewModel.RejectedInterviews.PropertyChanged += this.RejectedInterviews_PropertyChanged;
            this.ViewModel.CompletedInterviews.PropertyChanged += this.CompletedInterviews_PropertyChanged;

            this.ViewModel.TypeOfInterviews = this.ViewModel.CreateNew.InterviewStatus;

            this.fragmentStatePagerAdapter.AddFragment(typeof(QuestionnairesFragment), this.ViewModel.CreateNew,
                nameof(InterviewTabPanel.Title));

            this.UpdateFragmentByViewModelPropertyChange<StartedInterviewsFragment>(this.ViewModel.StartedInterviews);
            this.UpdateFragmentByViewModelPropertyChange<RejectedInterviewsFragment>(this.ViewModel.RejectedInterviews);
            this.UpdateFragmentByViewModelPropertyChange<CompletedInterviewsFragment>(this.ViewModel.CompletedInterviews);

            var tabLayout = this.FindViewById<TabLayout>(Resource.Id.tabs);
            tabLayout.SetupWithViewPager(this.viewPager);
        }

        private void CompletedInterviews_PropertyChanged(object sender, PropertyChangedEventArgs e)
            => this.UpdateFragmentByViewModelPropertyChange<CompletedInterviewsFragment>((CompletedInterviewsViewModel)sender, e.PropertyName);

        private void RejectedInterviews_PropertyChanged(object sender, PropertyChangedEventArgs e)
            => this.UpdateFragmentByViewModelPropertyChange<RejectedInterviewsFragment>((RejectedInterviewsViewModel)sender, e.PropertyName);

        private void StartedInterviews_PropertyChanged(object sender, PropertyChangedEventArgs e)
            => this.UpdateFragmentByViewModelPropertyChange<StartedInterviewsFragment>((StartedInterviewsViewModel)sender, e.PropertyName);

        private void UpdateFragmentByViewModelPropertyChange<TFragmentType>(ListViewModel<IDashboardItem> listViewModel,
            string propertyName = nameof(ListViewModel<IDashboardItem>.UiItems))
        {
            if (propertyName != nameof(ListViewModel<IDashboardItem>.UiItems)) return;

            if (!this.fragmentStatePagerAdapter.HasFragmentForViewModel(listViewModel) && listViewModel.Items != null &&
                listViewModel.Items.Count > 0)
            {
                this.fragmentStatePagerAdapter.AddFragment(typeof(TFragmentType), listViewModel,
                    nameof(InterviewTabPanel.Title));
            }

            if (this.fragmentStatePagerAdapter.HasFragmentForViewModel(listViewModel) && listViewModel.Items.Count == 0)
            {
                this.fragmentStatePagerAdapter.RemoveFragmentByViewModel(listViewModel);
            }
        }

        protected override void OnStart()
        {
            this.CreateFragments();
            base.OnStart();
            this.BindService(new Intent(this, typeof(SyncBgService)), new SyncServiceConnection(this), Bind.AutoCreate);
        }

        private void ViewPager_PageSelected(object sender, ViewPager.PageSelectedEventArgs e)
        {
            var currentFragment = (MvxFragment)this.fragmentStatePagerAdapter.GetItem(e.Position);

            this.ViewModel.TypeOfInterviews = ((InterviewTabPanel)currentFragment.ViewModel).InterviewStatus;
        }


        protected override void OnViewModelSet()
        {
            base.OnViewModelSet();
            this.ViewModel.Synchronization.SyncBgService = this;
        }

        protected override void OnDestroy()
        {
            this.RemoveFragments();

            base.OnDestroy();
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