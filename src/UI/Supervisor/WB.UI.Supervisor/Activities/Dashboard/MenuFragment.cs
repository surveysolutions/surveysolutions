using System;
using System.Reflection;
using System.Threading.Tasks;
using Android.OS;
using Android.Views;
using Android.Widget;
using Google.Android.Material.Navigation;
using MvvmCross.Core;
using MvvmCross.Platforms.Android.Binding.BindingContext;
using MvvmCross.Platforms.Android.Presenters.Attributes;
using MvvmCross.Platforms.Android.Views.Fragments;
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

        public override View OnCreateView(LayoutInflater inflater, ViewGroup container, Bundle savedInstanceState)
        {
            base.OnCreateView(inflater, container, savedInstanceState);

            var view = this.BindingInflate(Resource.Layout.dashboard_sidebar, null);

            navigationView = view.FindViewById<NavigationView>(Resource.Id.dashboard_sidebar_navigation);
            navigationView.SetNavigationItemSelectedListener(this);

            LocalizeMenuItem(Resource.Id.dashboard_to_be_assigned, SupervisorDashboard.ToBeAssigned, nameof(ViewModel.ToBeAssignedItemsCount));
            LocalizeMenuItem(Resource.Id.dashboard_your_team, SupervisorDashboard.YourTeam);
            LocalizeMenuItem(Resource.Id.dashboard_collected_interviews, SupervisorDashboard.CollectedInterviews);
            LocalizeMenuItem(Resource.Id.dashboard_waiting_decision, SupervisorDashboard.WaitingForAction, nameof(ViewModel.WaitingForDecisionCount));
            LocalizeMenuItem(Resource.Id.dashboard_outbox, SupervisorDashboard.Outbox, nameof(ViewModel.OutboxItemsCount));
            LocalizeMenuItem(Resource.Id.dashboard_sent, SupervisorDashboard.SentToInterviewer, nameof(ViewModel.SentToInterviewerCount));
            
            ViewModel.MvxNavigationService.DidNavigate += MenuFragment_AfterNavigate;
            
            return view;
        }

        public override void OnDestroyView()
        {
            ViewModel.MvxNavigationService.DidNavigate -= MenuFragment_AfterNavigate;
            base.OnDestroyView();
        }

        private void MenuFragment_AfterNavigate(object sender, MvvmCross.Navigation.EventArguments.IMvxNavigateEventArgs e)
        {
            int? menuItemId = null;
            switch (e.ViewModel)
            {
                case OutboxViewModel _:
                    menuItemId = Resource.Id.dashboard_outbox;
                    break;
                case ToBeAssignedItemsViewModel _:
                    menuItemId = Resource.Id.dashboard_to_be_assigned;
                    break;
                case WaitingForSupervisorActionViewModel _:
                    menuItemId = Resource.Id.dashboard_waiting_decision;
                    break;
                case SentToInterviewerViewModel _:
                    menuItemId = Resource.Id.dashboard_sent;
                    break;
            }

            if (menuItemId.HasValue)
                this.SelectMenuItem(navigationView.Menu.FindItem(menuItemId.Value));
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
            if (item == null) return;
            
            if (viewModelPropertyName != null)
            {
                item.SetActionView(Resource.Layout.dashboard_sidebar_counter);
                var textView = item.ActionView as TextView;
                if(textView == null) return;
                
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
            this.SelectMenuItem(item);
            this.Navigate(item.ItemId);

            return true;
        }

        private void SelectMenuItem(IMenuItem item)
        {
            this.Activity.RunOnUiThread(() =>
            {
                previousMenuItem?.SetChecked(false);

                item.SetCheckable(true);
                item.SetChecked(true);

                previousMenuItem = item;
            });
        }

        private async void Navigate(int itemId)
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
                case Resource.Id.dashboard_sent:
                    ViewModel.ShowSentItems.Execute();
                    break;
            }
        }
    }
}
