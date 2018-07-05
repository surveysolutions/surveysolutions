using System.ComponentModel;
using Android.App;
using Android.Content;
using Android.OS;
using Android.Support.Design.Widget;
using Android.Support.V4.View;
using Android.Support.V7.Widget;
using Android.Views;
using MvvmCross.Droid.Support.V4;
using WB.Core.BoundedContexts.Interviewer.Services;
using WB.Core.BoundedContexts.Interviewer.Views.Dashboard;
using WB.Core.SharedKernels.Enumerator.Properties;
using WB.Core.SharedKernels.Enumerator.Services;
using WB.Core.SharedKernels.Enumerator.Services.Synchronization;
using WB.Core.SharedKernels.Enumerator.ViewModels.Dashboard;
using WB.UI.Interviewer.Services;
using WB.UI.Shared.Enumerator.Activities;
using WB.UI.Shared.Enumerator.Services;
using MvxFragmentStatePagerAdapter = WB.UI.Interviewer.CustomControls.MvxFragmentStatePagerAdapter;

namespace WB.UI.Interviewer.Activities.Dashboard
{
    [Activity(Label = "", 
        Theme = "@style/GrayAppTheme", 
        WindowSoftInputMode = SoftInput.StateHidden,
        HardwareAccelerated = true,
        ConfigurationChanges = Android.Content.PM.ConfigChanges.Orientation | Android.Content.PM.ConfigChanges.ScreenSize)]
    public class DashboardActivity : BaseActivity<DashboardViewModel>, ISyncBgService<SyncProgressDto>, ISyncServiceHost<SyncBgService>
    {
        protected override int ViewResourceId => Resource.Layout.dashboard;

        public ServiceBinder<SyncBgService> Binder { get; set; }

        private MvxFragmentStatePagerAdapter fragmentStatePagerAdapter;
        private ViewPager viewPager;

        protected override void OnPause()
        {
            base.OnPause();
            this.RemoveFragments();
        }

        protected override void OnResume()
        {
            base.OnResume();
            this.CreateFragments();
        }

        protected override void OnCreate(Bundle bundle)
        {
            base.OnCreate(bundle);
            this.SetSupportActionBar(this.FindViewById<Toolbar>(Resource.Id.toolbar));
        }

        private void RemoveFragments()
        {
            this.fragmentStatePagerAdapter.RemoveAllFragments();
            this.fragmentStatePagerAdapter = null;
            this.viewPager.Adapter = null;

            this.viewPager.PageSelected -= this.ViewPager_PageSelected;

            this.ViewModel.StartedInterviews.PropertyChanged -= this.StartedInterviewsOnPropertyChanged;
            this.ViewModel.RejectedInterviews.PropertyChanged -= this.RejectedInterviewsOnPropertyChanged;
            this.ViewModel.CompletedInterviews.PropertyChanged -= this.CompletedInterviewsOnPropertyChanged;
        }

        private void CreateFragments()
        {
            this.viewPager = this.FindViewById<ViewPager>(Resource.Id.pager);

            this.fragmentStatePagerAdapter = new MvxFragmentStatePagerAdapter(this, this.SupportFragmentManager);
            this.viewPager.Adapter = this.fragmentStatePagerAdapter;
            this.viewPager.PageSelected += this.ViewPager_PageSelected;

            this.ViewModel.StartedInterviews.PropertyChanged += this.StartedInterviewsOnPropertyChanged;
            this.ViewModel.RejectedInterviews.PropertyChanged += this.RejectedInterviewsOnPropertyChanged;
            this.ViewModel.CompletedInterviews.PropertyChanged += this.CompletedInterviewsOnPropertyChanged;

            this.fragmentStatePagerAdapter.InsertFragment(typeof(QuestionnairesFragment), this.ViewModel.CreateNew,
                nameof(InterviewTabPanel.Title));

            var itemsCountPropertyCountName = nameof(ListViewModel.ItemsCount);

            this.StartedInterviewsOnPropertyChanged(this.ViewModel.StartedInterviews,
                new PropertyChangedEventArgs(itemsCountPropertyCountName));
            this.RejectedInterviewsOnPropertyChanged(this.ViewModel.RejectedInterviews,
                new PropertyChangedEventArgs(itemsCountPropertyCountName));
            this.CompletedInterviewsOnPropertyChanged(this.ViewModel.CompletedInterviews,
                new PropertyChangedEventArgs(itemsCountPropertyCountName));

            var tabLayout = this.FindViewById<TabLayout>(Resource.Id.tabs);
            tabLayout.SetupWithViewPager(this.viewPager);

            OpenRequestedTab();
        }

        private void OpenRequestedTab()
        {
            for (int i = 0; i < this.fragmentStatePagerAdapter.Count; i++)
            {
                var fragment = (MvxFragment) fragmentStatePagerAdapter.GetItem(i);
                InterviewTabPanel viewModel = (InterviewTabPanel) fragment.ViewModel;
                if (viewModel.InterviewStatus == this.ViewModel.TypeOfInterviews)
                {
                    this.viewPager.SetCurrentItem(i, false);
                    break;
                }
            }
        }

        private void CompletedInterviewsOnPropertyChanged(object sender, PropertyChangedEventArgs propertyChangedEventArgs)
            => this.UpdateFragmentByViewModelPropertyChange<CompletedInterviewsFragment>((ListViewModel)sender, propertyChangedEventArgs.PropertyName, 3);

        private void RejectedInterviewsOnPropertyChanged(object sender, PropertyChangedEventArgs propertyChangedEventArgs)
            => this.UpdateFragmentByViewModelPropertyChange<RejectedInterviewsFragment>((ListViewModel)sender, propertyChangedEventArgs.PropertyName, 2);

        private void StartedInterviewsOnPropertyChanged(object sender, PropertyChangedEventArgs propertyChangedEventArgs)
            => this.UpdateFragmentByViewModelPropertyChange<StartedInterviewsFragment>((ListViewModel)sender, propertyChangedEventArgs.PropertyName, 1);
        
        private void UpdateFragmentByViewModelPropertyChange<TFragmentType>(ListViewModel listViewModel, string propertyName, int position)
        {
            if (propertyName != nameof(ListViewModel.ItemsCount)) return;

            if (!this.fragmentStatePagerAdapter.HasFragmentForViewModel(listViewModel) && listViewModel.ItemsCount > 0)
            {
                this.fragmentStatePagerAdapter.InsertFragment(typeof(TFragmentType), listViewModel,
                    nameof(InterviewTabPanel.Title), position);
            }

            if (this.fragmentStatePagerAdapter.HasFragmentForViewModel(listViewModel) && listViewModel.ItemsCount == 0)
            {
                this.fragmentStatePagerAdapter.RemoveFragmentByViewModel(listViewModel);
            }


            var viewPagerCurrentItem = viewPager.CurrentItem;
            if (viewPagerCurrentItem > 0)
            {
                UpdateTypeOfInterviewsViewModelProperty(viewPagerCurrentItem);
            }
        }

        private void UpdateTypeOfInterviewsViewModelProperty(int tabPosition)
        {
            var fragment = (MvxFragment)this.fragmentStatePagerAdapter.GetItem(tabPosition);
            var viewModel = (ListViewModel)fragment.ViewModel;
            this.ViewModel.TypeOfInterviews = viewModel.InterviewStatus;
        }

        protected override void OnStart()
        {
            base.OnStart();
            this.BindService(new Intent(this, typeof(SyncBgService)), new SyncServiceConnection<SyncBgService>(this), Bind.AutoCreate);
        }

        private void ViewPager_PageSelected(object sender, ViewPager.PageSelectedEventArgs e)
        {
            UpdateTypeOfInterviewsViewModelProperty(e.Position);
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

            menu.LocalizeMenuItem(Resource.Id.menu_offline_synchronization, "Offline Sync");
            menu.LocalizeMenuItem(Resource.Id.menu_search, InterviewerUIResources.MenuItem_Title_Search);
            menu.LocalizeMenuItem(Resource.Id.menu_signout, InterviewerUIResources.MenuItem_Title_SignOut);
            menu.LocalizeMenuItem(Resource.Id.menu_settings, InterviewerUIResources.MenuItem_Title_Settings);
            menu.LocalizeMenuItem(Resource.Id.menu_diagnostics, InterviewerUIResources.MenuItem_Title_Diagnostics);
            menu.LocalizeMenuItem(Resource.Id.menu_maps, InterviewerUIResources.MenuItem_Title_Maps);
            return base.OnCreateOptionsMenu(menu);
        }

        public override bool OnOptionsItemSelected(IMenuItem item)
        {
            switch (item.ItemId)
            {
                case Resource.Id.menu_settings:
                    Intent intent = new Intent(this, typeof(PrefsActivity));
                    this.StartActivity(intent);
                    break;
                case Resource.Id.menu_diagnostics:
                    this.ViewModel.NavigateToDiagnosticsPageCommand.Execute();
                    break;
                case Resource.Id.menu_maps:
                    this.ViewModel.NavigateToMapsCommand.Execute();
                    break;
                case Resource.Id.menu_offline_synchronization:
                    this.ViewModel.NavigateToOfflineSyncCommand.Execute();
                    break;
                case Resource.Id.menu_signout:
                    this.ViewModel.SignOutCommand.Execute();
                    break;
                case Resource.Id.menu_search:
                    this.ViewModel.ShowSearchCommand.Execute();
                    break;
            }

            return base.OnOptionsItemSelected(item);
        }

        public void StartSync() => this.Binder.GetService().StartSync();

        public SyncProgressDto CurrentProgress => this.Binder.GetService().CurrentProgress;
    }
}
