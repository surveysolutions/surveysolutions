using Android.OS;
using Android.Views;
using MvvmCross.DroidX.RecyclerView;
using MvvmCross.Platforms.Android.Binding.BindingContext;
using WB.Core.SharedKernels.Enumerator.ViewModels.Dashboard;
using WB.UI.Shared.Enumerator.Activities;
using WB.UI.Shared.Enumerator.Activities.Dashboard;

namespace WB.UI.Interviewer.Activities.Dashboard
{
    public abstract class RecyclerViewFragment<TViewModel> : BaseFragment<TViewModel> where TViewModel : ListViewModel
    {
        protected override int ViewResourceId => Resource.Layout.dashboard_tab;

        public override View OnCreateView(LayoutInflater inflater, ViewGroup container, Bundle savedInstanceState)
        {
            var view = base.OnCreateView(inflater, container, savedInstanceState);

            var recyclerView = view.FindViewById<MvxRecyclerView>(Resource.Id.dashboard_tab_recycler);
            recyclerView.HasFixedSize = true;
            recyclerView.Adapter = new RecyclerViewAdapter((IMvxAndroidBindingContext)base.BindingContext);
            return view;
        }
    }
}
