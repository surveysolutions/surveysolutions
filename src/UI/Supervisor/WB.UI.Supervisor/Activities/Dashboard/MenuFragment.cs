using System;
using System.Reflection;
using System.Security.Permissions;
using System.Threading.Tasks;
using Android.OS;
using Android.Support.Design.Widget;
using Android.Support.V4.Widget;
using Android.Views;
using Android.Widget;
using MvvmCross.Core;
using MvvmCross.Droid.Support.V4;
using MvvmCross.Platforms.Android.Binding.BindingContext;
using MvvmCross.Platforms.Android.Presenters.Attributes;
using WB.Core.BoundedContexts.Supervisor.Properties;
using WB.Core.BoundedContexts.Supervisor.ViewModel.Dashboard;

namespace WB.UI.Supervisor.Activities.Dashboard
{
    [MvxFragmentPresentation(typeof(DashboardMenuViewModel), Resource.Id.navigation_frame,
        ActivityHostViewModelType = typeof(DashboardViewModel))]
    public class MenuFragment : MvxFragment<DashboardMenuViewModel>, NavigationView.IOnNavigationItemSelectedListener
    {
        private NavigationView navigationView;
        private IMenuItem previousMenuItem;
        private DrawerLayout drawerLayout;

        public override View OnCreateView(LayoutInflater inflater, ViewGroup container, Bundle savedInstanceState)
        {
            base.OnCreateView(inflater, container, savedInstanceState);

            var view = this.BindingInflate(Resource.Layout.dashboard_sidebar, null);

            navigationView = view.FindViewById<NavigationView>(Resource.Id.dashboard_sidebar_navigation);
            navigationView.SetNavigationItemSelectedListener(this);

            var tobeAssignedMenuItem = navigationView.Menu.FindItem(Resource.Id.dashboard_to_be_assigned);
            previousMenuItem = tobeAssignedMenuItem.SetChecked(true);

            LocalizeMenuItem(Resource.Id.dashboard_to_be_assigned, SupervisorDashboard.ToBeAssigned, nameof(ViewModel.ToBeAssignedItemsCount));
            LocalizeMenuItem(Resource.Id.dashboard_your_team, SupervisorDashboard.YourTeam);
            LocalizeMenuItem(Resource.Id.dashboard_collected_interviews, SupervisorDashboard.CollectedInterviews);
            LocalizeMenuItem(Resource.Id.dashboard_waiting_decision, SupervisorDashboard.WaitingForAction, nameof(ViewModel.WaitingForDecisionCount));
            LocalizeMenuItem(Resource.Id.dashboard_outbox, SupervisorDashboard.Outbox, nameof(ViewModel.OutboxItemsCount));

            return view;
        }

        private void LocalizeMenuItem(int id, string title, string viewModelPropertyName = null)
        {
            void SetLabelText(TextView textView, PropertyInfo viewModelPropertyInfo)
            {
                var value = ViewModel.GetPropertyValueAsString(viewModelPropertyInfo);
                if (value == "0")
                {
                    textView.Visibility = ViewStates.Gone;
                }
                else
                {
                    textView.Visibility = ViewStates.Visible;
                    textView.Text = value;
                }
            }

            var item = navigationView.Menu.FindItem(id);

            if (viewModelPropertyName != null)
            {
                item.SetActionView(Resource.Layout.dashboard_sidebar_counter);
                
                var textView = (TextView) item.ActionView;
                var viewModelPropertyInfo = ViewModel.GetType().GetProperty(viewModelPropertyName);

                SetLabelText(textView, viewModelPropertyInfo);

                ViewModel.PropertyChanged += (sender, args) =>
                {
                    if (args.PropertyName == viewModelPropertyName)
                    { 
                        SetLabelText(textView, viewModelPropertyInfo);
                    }
                };
            }

            item.SetTitle(title);
        }

        public bool OnNavigationItemSelected(IMenuItem item)
        {
            previousMenuItem?.SetChecked(false);

            item.SetCheckable(true);
            item.SetChecked(true);

            previousMenuItem = item;

#pragma warning disable 4014
            Navigate(item.ItemId);
#pragma warning restore 4014

            return true;
        }

        private async Task Navigate(int itemId)
        {
            ((DashboardActivity)Activity).DrawerLayout.CloseDrawers();
            await Task.Delay(TimeSpan.FromMilliseconds(250));

            switch(itemId)
            {
                case Resource.Id.dashboard_to_be_assigned:
                    ViewModel.ShowToBeAssignedItems.Execute();
                    break;
                case Resource.Id.dashboard_waiting_decision:
                    ViewModel.ShowWaitingForActionItems.Execute();
                    break;
                case Resource.Id.dashboard_outbox:
                    ViewModel.ShowOutboxItems.Execute();
                    break;
            }
        }
    }
}
