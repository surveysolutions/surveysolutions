using Android.OS;
using Android.Views;
using MvvmCross.Droid.Support.V7.RecyclerView;
using MvvmCross.Platforms.Android.Binding.BindingContext;
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
}