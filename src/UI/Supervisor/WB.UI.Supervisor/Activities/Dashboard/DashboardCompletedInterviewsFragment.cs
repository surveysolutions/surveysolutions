using Android.OS;
using Android.Runtime;
using Android.Views;
using MvvmCross.Droid.Support.V7.RecyclerView;
using MvvmCross.Platforms.Android.Binding.BindingContext;
using MvvmCross.Platforms.Android.Presenters.Attributes;
using WB.Core.BoundedContexts.Supervisor.ViewModel;
using WB.Core.SharedKernels.Enumerator.ViewModels.Dashboard;
using WB.UI.Shared.Enumerator.Activities;
using WB.UI.Shared.Enumerator.Activities.Dashboard;

namespace WB.UI.Supervisor.Activities.Dashboard
{
    public abstract class RecyclerViewFragment<TViewModel> : BaseFragment<TViewModel> where TViewModel : ListViewModel
    {
        protected override int ViewResourceId => Resource.Layout.dashboard_interviews;

        public override View OnCreateView(LayoutInflater inflater, ViewGroup container, Bundle savedInstanceState)
        {
            base.OnCreateView(inflater, container, savedInstanceState);

            var view = this.BindingInflate(this.ViewResourceId, null);

            var recyclerView = view.FindViewById<MvxRecyclerView>(Resource.Id.dashboard_tab_recycler);
            recyclerView.HasFixedSize = true;
            recyclerView.Adapter = new RecyclerViewAdapter((IMvxAndroidBindingContext)base.BindingContext);
            return view;
        }
    }

    [MvxFragmentPresentation(typeof(DashboardCompletedInterviewsViewModel), Resource.Id.dashboard_content,
        ActivityHostViewModelType = typeof(DashboardViewModel))]
    public class DashboardCompletedInterviewsFragment : RecyclerViewFragment<DashboardCompletedInterviewsViewModel>
    {
    }

    [MvxFragmentPresentation(typeof(DashboardRejectedInterviewsViewModel), Resource.Id.dashboard_content,
        ActivityHostViewModelType = typeof(DashboardViewModel))]
    public class DashboardRejectedInterviewsFragment : RecyclerViewFragment<DashboardRejectedInterviewsViewModel>
    {
    }

    [MvxFragmentPresentation(typeof(ToBeAssignedItemsViewModel), Resource.Id.dashboard_content,
        ActivityHostViewModelType = typeof(DashboardViewModel))]
    public class DashboardToBeAssignedFragment : RecyclerViewFragment<ToBeAssignedItemsViewModel>
    {
    }
}
