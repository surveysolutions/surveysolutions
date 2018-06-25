using System;
using System.Threading.Tasks;
using Android.OS;
using Android.Support.Design.Widget;
using Android.Views;
using MvvmCross.Droid.Support.V4;
using MvvmCross.Platforms.Android.Binding.BindingContext;
using MvvmCross.Platforms.Android.Presenters.Attributes;
using WB.Core.BoundedContexts.Supervisor.ViewModel;
using WB.UI.Supervisor.Properties;

namespace WB.UI.Supervisor.Activities.Dashboard
{
    [MvxFragmentPresentation(typeof(DashboardMenuViewModel), Resource.Id.navigation_frame,
        ActivityHostViewModelType = typeof(DashboardViewModel))]
    public class MenuFragment : MvxFragment<DashboardMenuViewModel>, NavigationView.IOnNavigationItemSelectedListener
    {
        private NavigationView navigationView;
        private IMenuItem _previousMenuItem;

        public override View OnCreateView(LayoutInflater inflater, ViewGroup container, Bundle savedInstanceState)
        {
            base.OnCreateView(inflater, container, savedInstanceState);

            var view = this.BindingInflate(Resource.Layout.dashboard_sidebar, null);

            navigationView = view.FindViewById<NavigationView>(Resource.Id.dashboard_sidebar_navigation);
            navigationView.SetNavigationItemSelectedListener(this);

            var interviewsMenuItem = navigationView.Menu.FindItem(Resource.Id.dashboard_completed_interviews);
            interviewsMenuItem.SetTitle(SupervisorDashboard.Completed);

            var rejectedMenuItem = navigationView.Menu.FindItem(Resource.Id.dashboard_rejected_interviews);
            rejectedMenuItem.SetTitle(SupervisorDashboard.Rejected);

            var assignmentsMenuItem = navigationView.Menu.FindItem(Resource.Id.dashboard_assignments);
            assignmentsMenuItem.SetTitle(SupervisorDashboard.Assignments);

            var approvedMenuItem = navigationView.Menu.FindItem(Resource.Id.dashboard_approved_interviews);
            approvedMenuItem.SetTitle(SupervisorDashboard.Assignments);

            _previousMenuItem = interviewsMenuItem.SetChecked(true);

            return view;
        }

        public bool OnNavigationItemSelected(IMenuItem item)
        {
            _previousMenuItem?.SetChecked(false);

            item.SetCheckable(true);
            item.SetChecked(true);

            _previousMenuItem = item;

#pragma warning disable 4014
            Navigate(item.ItemId);
#pragma warning restore 4014

            return true;
        }

        
        private async Task Navigate(int itemId)
        {
            //((AppCompat)Activity).DrawerLayout.CloseDrawers();
            await Task.Delay(TimeSpan.FromMilliseconds(250));

            switch(itemId)
            {
                case Resource.Id.dashboard_completed_interviews:
                    ViewModel.ShowCompletedInterviews.Execute();
                    break;
            }
        }
    }
}
