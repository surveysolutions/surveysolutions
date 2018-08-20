using Android.App;
using Android.OS;
using Android.Views;
using MvvmCross.Droid.Support.V7.RecyclerView;
using MvvmCross.Platforms.Android.Binding.BindingContext;
using WB.Core.SharedKernels.Enumerator.ViewModels;
using WB.UI.Shared.Enumerator.Activities.Dashboard;

namespace WB.UI.Shared.Enumerator.Activities
{
    public abstract class SearchActivity : BaseActivity<SearchViewModel>
    {
        protected override int ViewResourceId => Resource.Layout.search;

        protected override void OnCreate(Bundle bundle)
        {
            base.OnCreate(bundle);

            var recyclerView = this.FindViewById<MvxRecyclerView>(Resource.Id.dashboard_tab_recycler);
            recyclerView.HasFixedSize = true;
            recyclerView.Adapter = new RecyclerViewAdapter((IMvxAndroidBindingContext)base.BindingContext);
        }
    }
}
