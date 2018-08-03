using Android.OS;
using Android.Views;
using MvvmCross.Droid.Support.V4;
using MvvmCross.Platforms.Android.Binding.BindingContext;
using MvvmCross.Platforms.Android.Presenters.Attributes;
using WB.Core.SharedKernels.Enumerator.ViewModels.InterviewDetails.Overview;

namespace WB.UI.Shared.Enumerator.Activities
{
    [MvxDialogFragmentPresentation]
    public class OverviewNodeDetailsFragment: MvxDialogFragment<OverviewNodeDetailsViewModel>
    {
        protected int ViewResourceId => Resource.Layout.interview_overview_node_details;

        public override View OnCreateView(LayoutInflater inflater, ViewGroup container, Bundle savedInstanceState)
        {
            base.OnCreateView(inflater, container, savedInstanceState);
            return this.BindingInflate(ViewResourceId, container, false);
        }
    }
}
